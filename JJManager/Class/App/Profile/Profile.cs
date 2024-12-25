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
using JJDeviceClass = JJManager.Class.Devices.JJDevice;

namespace JJManager.Class.App.Profile
{
    public class Profile : INotifyCollectionChanged
    {
        private String _id = "";
        private String _name = "";
        private String _idProduct = "";
        private int _analogInputsQtd = 0;
        private int _digitalInputsQtd = 0;
        private JsonObject _data = null;
        private ObservableCollection<InputClass> _inputs = null;
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

                // Insere um novo perfil se nenhum for encontrado
                if (string.IsNullOrEmpty(_id))
                {
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
                            jjp.digital_inputs_qtd 
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
                _data = obj.ContainsKey("configs") ? (JsonObject)JsonObject.Parse(obj["configs"].GetValue<string>()) : new JsonObject();

                uint uid = uint.Parse(_id);
                _inputs = new ObservableCollection<InputClass>();

                for (uint j = 0; j < (_analogInputsQtd + _digitalInputsQtd); j++)
                {
                    _inputs.Add(new InputClass(uid, j));
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
                            jjp.digital_inputs_qtd 
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
                _data = obj.ContainsKey("configs") ? (JsonObject)JsonObject.Parse(obj["configs"].GetValue<string>()) : new JsonObject();

                uint uid = uint.Parse(_id);
                _inputs = new ObservableCollection<InputClass>();

                for (uint j = 0; j < (_analogInputsQtd + _digitalInputsQtd); j++)
                {
                    _inputs.Add(new InputClass(uid, j));
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
                            jjp.digital_inputs_qtd
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
                _data = obj.ContainsKey("configs") ? (JsonObject)JsonObject.Parse(obj["configs"].GetValue<string>()) : new JsonObject();

                uint uid = uint.Parse(_id);
                _inputs = new ObservableCollection<InputClass>();

                for (uint j = 0; j < (_analogInputsQtd + _digitalInputsQtd); j++)
                {
                    _inputs.Add(new InputClass(uid, j));
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
        public void Delete(JJDeviceClass device, string profileNameToActive)
        {
            _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;
            SetToActiveProfile(profileNameToActive);

            string sql = $"DELETE FROM dbo.profiles WHERE name = '{_name}' AND id_product = '{device.ProductId}';";

            if (!_dbConnection.RunSQL(sql))
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
