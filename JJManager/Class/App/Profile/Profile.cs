using JJManager.Class.App.Input;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using InputClass = JJManager.Class.App.Input.Input;
using OutputClass = JJManager.Class.App.Output.Output;
using JJDeviceClass = JJManager.Class.Devices.JJDevice;
using System.Linq;
using static JJManager.Class.App.Output.Output;
using System.Windows.Media.Animation;

namespace JJManager.Class.App.Profile
{
    public class Profile : INotifyCollectionChanged
    {
        private String _id = "";
        private String _name = "";
        private String _idProduct = "";
        private int _analogInputsQtd = 0;
        private int _digitalInputsQtd = 0;
        private int _analogOutputsQtd = 0;
        private int _digitalOutputsQtd = 0;
        private JsonObject _data = null;
        private ObservableCollection<InputClass> _inputs = null;
        private ObservableCollection<OutputClass> _outputs = null;
        private DatabaseConnection _dbConnection = null;
        private bool _needsUpdate = false;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public bool NeedsUpdate
        {
            get => _needsUpdate;
            set => _needsUpdate = value;
        }

        public String Id
        {
            get => _id;
        }

        public String IdProduct
        {
            get => _idProduct;
        }

        public String Name
        {
            get => _name;
        }

        public ObservableCollection<InputClass> Inputs
        {
            get => _inputs;
        }
        public ObservableCollection<OutputClass> Outputs
        {
            get => _outputs;
        }

        public JsonObject Data
        {
            get => _data;
        }

        #region constructors
        /// <summary>
        /// Constructor responsable to get a profile using there ID.
        /// </summary>
        /// <param name="id">Profile's ID.</param>
        public Profile(string id)
        {
            _dbConnection = new DatabaseConnection();
            GetProfileIntoObject(id);
        }

        /// <summary>
        /// Constructor responsable to create or get a profile based in data passed.
        /// </summary>
        /// <param name="device">JJManager device object.</param>
        /// <param name="name">New profile's name.</param>
        /// <param name="activeProfile">Bool, True if it's the active profile, false if not.</param>
        public Profile(JJDeviceClass device, string profileName, bool setToActiveProfile = false)
        {
            CreateNewProfileIntoObject(device, profileName, setToActiveProfile);
        }

        /// <summary>
        /// Get device's active profile
        /// </summary>
        /// <param name="device"></param>
        public Profile(JJDeviceClass device)
        {
            try
            {
                if (device == null)
                {
                    throw new ArgumentNullException(nameof(device), "O objeto dispositivo não pode ser nulo.");
                }

                if (string.IsNullOrEmpty(device.ConnId) || string.IsNullOrEmpty(device.ProductId))
                {
                    throw new ArgumentException("O dispositivo não possui ConnId ou ProductId válido.");
                }

                // Inicializa conexão com o banco de dados
                _dbConnection = _dbConnection ?? new DatabaseConnection();

                // Tenta buscar perfil ativo
                GetActiveProfileIntoObject(device.ConnId);

                // Se nenhum perfil ativo foi encontrado
                if (string.IsNullOrEmpty(_id))
                {
                    // Primeiro, verifica se já existe um perfil padrão para este modelo de dispositivo
                    string checkDefaultProfileSql = $"SELECT TOP 1 id FROM profiles WHERE id_product = '{device.ProductId}' AND name = 'Perfil Padrão' ORDER BY id ASC;";
                    var existingDefaultProfile = _dbConnection.RunSQLWithResults(checkDefaultProfileSql);

                    if (existingDefaultProfile.Count > 0)
                    {
                        JsonObject firstResult = (JsonObject)existingDefaultProfile[0];
                        if (firstResult.ContainsKey("id"))
                        {
                            // Usa o perfil padrão existente
                            _id = firstResult["id"].GetValue<string>();
                            GetProfileIntoObject(_id);

                            // Associa este perfil ao dispositivo atual
                            SetToActiveProfile(device.ConnId);

                            Log.Insert("Profile", $"Usando perfil padrão existente (ID: {_id}) para o dispositivo {device.ProductId}");
                        }
                    }
                    else
                    {
                        // Cria um novo perfil padrão se não existir
                        string insertSql = $"INSERT INTO profiles (name, id_product) VALUES ('Perfil Padrão', '{device.ProductId}');";

                        if (!_dbConnection.RunSQL(insertSql))
                        {
                            Log.Insert("Profile", "Erro ao inserir perfil no banco de dados");
                            throw new InvalidOperationException("Falha ao inserir perfil no banco de dados.");
                        }

                        string selectSql = "SELECT IDENT_CURRENT('dbo.profiles') AS last_inserted_id";
                        var results = _dbConnection.RunSQLWithResults(selectSql);

                        if (results.Count == 0)
                        {
                            throw new InvalidOperationException("Falha ao recuperar o ID do último perfil inserido.");
                        }

                        foreach (JsonObject obj in results)
                        {
                            if (obj.ContainsKey("last_inserted_id"))
                            {
                                _id = obj["last_inserted_id"].GetValue<string>();
                                GetProfileIntoObject(_id);

                                // Associa este novo perfil ao dispositivo
                                SetToActiveProfile(device.ConnId);

                                Log.Insert("Profile", $"Criado novo perfil padrão (ID: {_id}) para o dispositivo {device.ProductId}");
                            }
                        }
                    }
                }

                // Verifica se o perfil foi encontrado ou criado
                if (string.IsNullOrEmpty(_id))
                {
                    throw new NullReferenceException("O ID do perfil não foi encontrado ou criado.");
                }

                // Converte o ID do perfil para inteiro e inicializa entradas
                uint uid = uint.Parse(_id);
                _inputs = new ObservableCollection<InputClass>();

                for (uint j = 0; j < (_analogInputsQtd + _digitalInputsQtd); j++)
                {
                    _inputs.Add(new InputClass(uid, j));
                }
            }
            catch (ArgumentNullException ex)
            {
                Log.Insert("Profile", "Erro ao criar o perfil: parâmetro nulo.", ex);
            }
            catch (ArgumentException ex)
            {
                Log.Insert("Profile", "Erro ao criar o perfil: parâmetro inválido.", ex);
            }
            catch (InvalidOperationException ex)
            {
                Log.Insert("Profile", "Erro ao criar o perfil: operação inválida.", ex);
            }
            catch (NullReferenceException ex)
            {
                Log.Insert("Profile", "Erro ao buscar o perfil atrelado ao dispositivo.", ex);
            }
            catch (Exception ex)
            {
                Log.Insert("Profile", "Erro inesperado ao criar o perfil.", ex);
            }
        }
        #endregion

        #region FunctionsToManipulateData
        private void SetToActiveProfile(string connId)
        {
            _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;

            string sql = $"UPDATE dbo.user_products SET id_profile = '{_id}' WHERE conn_id = '{connId}'";

            if (!_dbConnection.RunSQL(sql))
            {
                // TODO: Create LOGFILE
            }
        }

        private void GetProfileIntoObject(string id)
        {
            _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;

            String sql = $@"SELECT 
                            p.name, 
                            p.id_product,
                            p.configs, 
                            jjp.analog_inputs_qtd,
                            jjp.digital_inputs_qtd, 
                            jjp.analog_outputs_qtd,
                            jjp.digital_outputs_qtd 
                        FROM profiles AS p 
                        LEFT JOIN jj_products AS jjp ON (p.id_product = jjp.id) 
                        WHERE p.id = '{id}';";

            foreach (JsonObject obj in _dbConnection.RunSQLWithResults(sql))
            {
                _id = id;
                _name = obj.ContainsKey("name") ? obj["name"].GetValue<string>() : string.Empty;
                _idProduct = obj.ContainsKey("id_product") ? obj["id_product"].GetValue<string>() : string.Empty;
                _analogInputsQtd = obj.ContainsKey("analog_inputs_qtd") ? Int32.Parse(obj["analog_inputs_qtd"].GetValue<string>()) : 0;
                _digitalInputsQtd = obj.ContainsKey("digital_inputs_qtd") ? Int32.Parse(obj["digital_inputs_qtd"].GetValue<string>()) : 0;
                _analogOutputsQtd = obj.ContainsKey("analog_outputs_qtd") ? Int32.Parse(obj["analog_outputs_qtd"].GetValue<string>()) : 0;
                _digitalOutputsQtd = obj.ContainsKey("digital_outputs_qtd") ? Int32.Parse(obj["digital_outputs_qtd"].GetValue<string>()) : 0;
                _data = obj.ContainsKey("configs") ? (JsonObject)JsonObject.Parse(obj["configs"].GetValue<string>()) : new JsonObject();

                uint uid = uint.Parse(_id);
                _inputs = new ObservableCollection<InputClass>();

                for (uint j = 0; j < (_analogInputsQtd + _digitalInputsQtd); j++)
                {
                    _inputs.Add(new InputClass(uid, j));
                }

                _outputs = new ObservableCollection<OutputClass>();

                for (uint j = 0; j < (_analogOutputsQtd + _digitalOutputsQtd); j++)
                {
                    _outputs.Add(new OutputClass(uid, j));
                }
            }
        }

        private void GetActiveProfileIntoObject(string connId)
        {
            _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;
            String sql = $@"SELECT 
                            p.id, 
                            p.name, 
                            p.id_product,
                            p.configs, 
                            jjp.analog_inputs_qtd,
                            jjp.digital_inputs_qtd, 
                            jjp.analog_outputs_qtd,
                            jjp.digital_outputs_qtd 
                        FROM profiles AS p 
                        LEFT JOIN jj_products AS jjp ON (p.id_product = jjp.id) 
                        LEFT JOIN user_products AS up ON (p.id = up.id_profile) 
                        WHERE up.conn_id = '{connId}';";

            foreach (JsonObject obj in _dbConnection.RunSQLWithResults(sql))
            {
                _id = obj.ContainsKey("id") ? obj["id"].GetValue<string>() : string.Empty;
                _name = obj.ContainsKey("name") ? obj["name"].GetValue<string>() : string.Empty;
                _idProduct = obj.ContainsKey("id_product") ? obj["id_product"].GetValue<string>() : string.Empty;
                _analogInputsQtd = obj.ContainsKey("analog_inputs_qtd") ? Int32.Parse(obj["analog_inputs_qtd"].GetValue<string>()) : 0;
                _digitalInputsQtd = obj.ContainsKey("digital_inputs_qtd") ? Int32.Parse(obj["digital_inputs_qtd"].GetValue<string>()) : 0;
                _analogOutputsQtd = obj.ContainsKey("analog_outputs_qtd") ? Int32.Parse(obj["analog_outputs_qtd"].GetValue<string>()) : 0;
                _digitalOutputsQtd = obj.ContainsKey("digital_outputs_qtd") ? Int32.Parse(obj["digital_outputs_qtd"].GetValue<string>()) : 0;
                _data = obj.ContainsKey("configs") ? (JsonObject)JsonObject.Parse(obj["configs"].GetValue<string>()) : new JsonObject();

                uint uid = uint.Parse(_id);
                _inputs = new ObservableCollection<InputClass>();

                for (uint j = 0; j < (_analogInputsQtd + _digitalInputsQtd); j++)
                {
                    _inputs.Add(new InputClass(uid, j));
                }

                _outputs = new ObservableCollection<OutputClass>();

                for (uint j = 0; j < (_analogOutputsQtd + _digitalOutputsQtd); j++)
                {
                    _outputs.Add(new OutputClass(uid, j));
                }
            }
        }

        private void GetProfileIntoObject(string name, string idProduct)
        {
            _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;
            string sql = $@"SELECT
                            p.id,
                            p.configs,
                            jjp.analog_inputs_qtd,
                            jjp.digital_inputs_qtd, 
                            jjp.analog_outputs_qtd,
                            jjp.digital_outputs_qtd 
                        FROM profiles AS p 
                        LEFT JOIN jj_products AS jjp ON (p.id_product = jjp.id) 
                        WHERE name = '{name}' AND id_product = '{idProduct}';";

            foreach (JsonObject obj in _dbConnection.RunSQLWithResults(sql))
            {
                _id = obj.ContainsKey("id") ? obj["id"].GetValue<string>() : string.Empty;
                _name = name;
                _idProduct = idProduct;
                _analogInputsQtd = obj.ContainsKey("analog_inputs_qtd") ? Int32.Parse(obj["analog_inputs_qtd"].GetValue<string>()) : 0;
                _digitalInputsQtd = obj.ContainsKey("digital_inputs_qtd") ? Int32.Parse(obj["digital_inputs_qtd"].GetValue<string>()) : 0;
                _analogOutputsQtd = obj.ContainsKey("analog_outputs_qtd") ? Int32.Parse(obj["analog_outputs_qtd"].GetValue<string>()) : 0;
                _digitalOutputsQtd = obj.ContainsKey("digital_outputs_qtd") ? Int32.Parse(obj["digital_outputs_qtd"].GetValue<string>()) : 0;
                _data = obj.ContainsKey("configs") ? (JsonObject)JsonObject.Parse(obj["configs"].GetValue<string>()) : new JsonObject();

                uint uid = uint.Parse(_id);
                _inputs = new ObservableCollection<InputClass>();

                for (uint j = 0; j < (_analogInputsQtd + _digitalInputsQtd); j++)
                {
                    _inputs.Add(new InputClass(uid, j));
                }

                _outputs = new ObservableCollection<OutputClass>();

                for (uint j = 0; j < (_analogOutputsQtd + _digitalOutputsQtd); j++)
                {
                    _outputs.Add(new OutputClass(uid, j));
                }
            }
        }

        private void CreateRestartAllProfileFile()
        {
            string pathAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JohnJohn3D", "JJManager");
            string restartProfileFile = Path.Combine(pathAppData, "RestartProfileFile");

            if (!Directory.Exists(pathAppData))
                Directory.CreateDirectory(pathAppData);

            if (!File.Exists(restartProfileFile))
                File.Create(restartProfileFile);
        }
        #endregion

        #region PublicFunctions
        //public void Delete(JJDeviceClass device, string profileNameToActive)
        //{
        //    _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;

        //    string sql = $"DELETE FROM dbo.profiles WHERE name = '{device.Profile.Name}' AND id_product = '{device.ProductId}';";

        //    SetToActiveProfile(profileNameToActive);

        //    if (!_dbConnection.RunSQL(sql))
        //    {
        //        // TODO: Create LOGFILE
        //    }
        //}

        public static void Delete(string profileNameToExclude, string productId)
        {
            DatabaseConnection dbConnection = new DatabaseConnection();
            
            string sql = $"DELETE FROM dbo.profiles WHERE name = '{profileNameToExclude}' AND id_product = '{productId}';";

            //SetToActiveProfile(profileNameToActive);

            if (!dbConnection.RunSQL(sql))
            {
                // TODO: Create LOGFILE
            }
        }


        public void Update(JsonObject dataToUpdate)
        {
            _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;

            if (dataToUpdate.ContainsKey("name"))
            {
                _name = dataToUpdate["name"].GetValue<string>();
            }

            if (dataToUpdate.ContainsKey("data"))
            {
                _data = dataToUpdate["data"].AsObject();
            }

            string sql = $@"UPDATE dbo.profiles SET 
                            name = '{_name}',
                            configs = '{_data.ToJsonString()}'
                        WHERE id = '{_id}';";

            if (!_dbConnection.RunSQL(sql))
            {
                // TODO: Create LOGFILE
            }
        }

        public void Restart()
        {
            _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;
            GetProfileIntoObject(_id);
            CreateRestartAllProfileFile();
        }

        public void UpdateInput(InputClass input)
        {
            if (input == null)
            {
                return;
            }

            input.Save();
            _inputs[(int)input.Id] = input;
            CreateRestartAllProfileFile();
        }

        public void UpdateOutput(OutputClass output)
        {
            if (output == null)
            {
                return;
            }

            output.Save();
            _outputs[(int)output.Id] = output;
            CreateRestartAllProfileFile();
        }

        public void OrderOutputsBy(OutputClass.OutputMode mode, int ledSelected = -1)
        {
            if (mode == OutputClass.OutputMode.Leds)
            {
                // Sort directly within the existing collection
                var sortedList = _outputs.Where(x => x.Led != null && (ledSelected > -1 ? x.Led?.LedsGrouped?.Contains(ledSelected) ?? false : true)).OrderBy(x => x.Led?.Order ?? int.MaxValue)
                                         .ToList();

                int newPos = 0;

                foreach (var output in sortedList)
                {
                    int index = _outputs.IndexOf(output);
                    _outputs[index].Led.Order = newPos++;
                    _outputs[index].Changed = true;

                }

                for (int i = 0; i < _outputs.Count; i++)
                {
                    
                }
            }
        }

        public void MoveOutput(uint id, int positionToMove, int positionsAvailableOnGrid, OutputClass.OutputMode mode)
        {
            if (positionToMove < 0 || positionToMove > positionsAvailableOnGrid)
            {
                return;
            }

            if (mode == OutputClass.OutputMode.Leds)
            {
                var movingOutput = _outputs.FirstOrDefault(o => o.Mode == mode && o.Id == id);
                if (movingOutput == null || movingOutput.Led == null) return;

                foreach (int ledPos in movingOutput.Led.LedsGrouped)
                {
                    OrderOutputsBy(mode, ledPos);
                }

                int actualPosition = movingOutput.Led.Order;

                // No movement needed
                if (actualPosition == positionToMove) return;

                // Moving Down (i.e., increasing order)
                if (positionToMove > actualPosition)
                {
                    foreach (var output in _outputs.Where(o =>
                        o.Mode == mode &&
                        o.Led != null &&
                        o.Led.Order > actualPosition &&
                        o.Led.Order <= positionToMove &&
                        o.Led.LedsGrouped.Any(x => movingOutput.Led.LedsGrouped.Contains(x))))
                    {
                        output.Led.Order--;
                        output.Changed = true;
                    }
                }
                // Moving Up (i.e., decreasing order)
                else if (positionToMove < actualPosition)
                {
                    foreach (var output in _outputs.Where(o =>
                        o.Mode == mode &&
                        o.Led != null &&
                        o.Led.Order >= positionToMove &&
                        o.Led.Order < actualPosition &&
                        o.Led.LedsGrouped.Any(x => movingOutput.Led.LedsGrouped.Contains(x))))
                    {
                        output.Led.Order++;
                        output.Changed = true;
                    }
                }

                // Set new order for the moving output
                movingOutput.Led.Order = positionToMove;
                movingOutput.Changed = true;

                foreach (int ledPos in movingOutput.Led.LedsGrouped)
                {
                    OrderOutputsBy(mode, ledPos);
                }

                for (int i = 0; i < _outputs.Count; i++) 
                {
                    if (!_outputs[i].Changed)
                    {
                        continue;
                    }

                    _outputs[i].Save();
                    _outputs[i].Changed = false;
                }
            }
        }


        public void ExcludeOutput(int id, OutputClass.OutputMode mode)
        {
            if (id < 0 || id > _analogOutputsQtd + _digitalOutputsQtd)
            {
                return;
            }

            for (int i = 0; i < _outputs.Count; i++)
            {
                if (_outputs[i].Id == id)
                {
                    if (_outputs[i].Mode == OutputMode.Leds)
                    {
                        List<int> ledsToReorder = _outputs[i].Led.LedsGrouped;
                        _outputs[i].RemoveFunction();
                        _outputs[i].Changed = true; // Mark as changed to trigger Save

                        foreach (int ledPos in ledsToReorder)
                        {
                            OrderOutputsBy(mode, ledPos);
                        }
                    }
                    break;
                }
            }

            for (int i = 0; i < _outputs.Count; i++)
            {
                if (!_outputs[i].Changed)
                {
                    continue;
                }

                _outputs[i].Save();
                _outputs[i].Changed = false;
            }

            CreateRestartAllProfileFile();
        }

        public void CreateNewProfileIntoObject(JJDeviceClass device, string profileName, bool setToActiveProfile = false)
        {
            try
            {
                _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;
                GetProfileIntoObject(profileName, device.ProductId);

                // If not found a ID...
                if (string.IsNullOrEmpty(_id))
                {
                    string sql = $"INSERT INTO profiles (name, id_product) VALUES ('{profileName}', '{device.ProductId}');";

                    if (!_dbConnection.RunSQL(sql))
                    {
                        // TODO: Create LOGFILE
                    }

                    sql = "SELECT IDENT_CURRENT('dbo.profiles') AS last_inserted_id";

                    foreach (JsonObject obj in _dbConnection.RunSQLWithResults(sql))
                    {
                        if (obj.ContainsKey("last_inserted_id"))
                        {
                            GetProfileIntoObject(obj["last_inserted_id"].GetValue<string>());
                        }
                    }
                }

                // If not found a ID yet...
                if (string.IsNullOrEmpty(_id))
                {
                    throw new NullReferenceException();
                }

                uint uid = uint.Parse(_id);
                _inputs = new ObservableCollection<InputClass>();

                for (uint j = 0; j < (_analogInputsQtd + _digitalInputsQtd); j++)
                {
                    _inputs.Add(new InputClass(uid, j));
                }

                _outputs = new ObservableCollection<OutputClass>();

                for (uint j = 0; j < (_analogOutputsQtd + _digitalOutputsQtd); j++)
                {
                    _outputs.Add(new OutputClass(uid, j));
                }

                if (setToActiveProfile)
                {
                    SetToActiveProfile(device.ConnId);
                }
            }
            catch (NullReferenceException ex)
            {
                Log.Insert("Profile", "Erro ao criar/buscar o usuário", ex);
            }
        }
        #endregion

        #region StaticFunctions
        public static List<String> GetProfilesList(String productId)
        {
            DatabaseConnection database = new DatabaseConnection();

            List<String> list = new List<String>();
            String sql = "SELECT name FROM profiles WHERE id_product = " + productId + " ORDER BY id ASC;";

            foreach (JsonObject obj in database.RunSQLWithResults(sql))
            {
                if (obj.ContainsKey("name"))
                {
                    list.Add(obj["name"].GetValue<string>());
                }
            }

            return list;
        }
        #endregion
    }
}
