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

        public event NotifyCollectionChangedEventHandler CollectionChanged;
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
        public Profile(Device device, string profileName, bool setToActiveProfile = false)
        {
            CreateNewProfileIntoObject(device, profileName, setToActiveProfile);
        }

        /// <summary>
        /// Get device's active profile
        /// </summary>
        /// <param name="device"></param>
        public Profile(Device device)
        {
            try
            {
                _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;
                GetActiveProfileIntoObject(device.ConnId);

                // If not found a ID...
                if (string.IsNullOrEmpty(_id))
                {
                    string sql = $"INSERT INTO profiles (name, id_product) VALUES ('Perfil Padrão', '{device.JJID}');";

                    if (!_dbConnection.RunSQL(sql))
                    {
                        // TODO: Create LOGFILE
                    }

                    sql = "SELECT IDENT_CURRENT('dbo.profiles') AS last_inserted_id";

                    using (JsonDocument json = _dbConnection.RunSQLWithResults(sql))
                    {
                        if (json != null)
                        {
                            GetProfileIntoObject(json.RootElement[0].GetProperty("last_inserted_id").GetString());
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
            }
            catch (NullReferenceException ex)
            {
                Log.Insert("Profile", "Erro ao buscar o perfil atrelado ao dispositivo", ex);
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

            using (JsonDocument json = _dbConnection.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    _id = id;
                    _name = json.RootElement[0].GetProperty("name").GetString();
                    _idProduct = json.RootElement[0].GetProperty("id_product").GetString();
                    _analogInputsQtd = Int32.Parse(json.RootElement[0].GetProperty("analog_inputs_qtd").GetString());
                    _digitalInputsQtd = Int32.Parse(json.RootElement[0].GetProperty("digital_inputs_qtd").GetString());

                    if (json.RootElement[0].TryGetProperty("configs", out JsonElement config))
                    {
                        _data = (JsonObject)JsonObject.Parse(config.GetString());
                    }
                    else
                    {
                        _data = new JsonObject();
                    }

                    uint uid = uint.Parse(_id);
                    _inputs = new ObservableCollection<InputClass>();

                    for (uint j = 0; j < (_analogInputsQtd + _digitalInputsQtd); j++)
                    {
                        _inputs.Add(new InputClass(uid, j));
                    }
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

            using (JsonDocument json = _dbConnection.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    _id = json.RootElement[0].GetProperty("id").GetString();
                    _name = json.RootElement[0].GetProperty("name").GetString();
                    _idProduct = json.RootElement[0].GetProperty("id_product").GetString();
                    _analogInputsQtd = Int32.Parse(json.RootElement[0].GetProperty("analog_inputs_qtd").GetString());
                    _digitalInputsQtd = Int32.Parse(json.RootElement[0].GetProperty("digital_inputs_qtd").GetString());

                    if (json.RootElement[0].TryGetProperty("configs", out JsonElement config))
                    {
                        _data = (JsonObject)JsonObject.Parse(config.GetString());
                    }
                    else
                    {
                        _data = new JsonObject();
                    }

                    uint uid = uint.Parse(_id);
                    _inputs = new ObservableCollection<InputClass>();

                    for (uint j = 0; j < (_analogInputsQtd + _digitalInputsQtd); j++)
                    {
                        _inputs.Add(new InputClass(uid, j));
                    }
                }
            }
        }

        private void GetProfileIntoObject(string name, string idProduct)
        {
            _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;
            string sql = $@"SELECT
                            p.id,
                            p.name,
                            p.configs,
                            jjp.analog_inputs_qtd,
                            jjp.digital_inputs_qtd
                        FROM profiles AS p 
                        LEFT JOIN jj_products AS jjp ON (p.id_product = jjp.id) 
                        WHERE name = '{name}' AND id_product = '{idProduct}';";

            using (JsonDocument json = _dbConnection.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    _id = json.RootElement[0].GetProperty("id").ToString();
                    _name = json.RootElement[0].GetProperty("name").GetString();
                    _idProduct = idProduct;
                    _analogInputsQtd = Int32.Parse(json.RootElement[0].GetProperty("analog_inputs_qtd").GetString());
                    _digitalInputsQtd = Int32.Parse(json.RootElement[0].GetProperty("digital_inputs_qtd").GetString());
                    if (json.RootElement[0].TryGetProperty("configs", out JsonElement config))
                    {
                        _data = (JsonObject)JsonObject.Parse(config.GetString());
                    }
                    else
                    {
                        _data = new JsonObject();
                    }

                    uint uid = uint.Parse(_id);
                    _inputs = new ObservableCollection<InputClass>();

                    for (uint j = 0; j < (_analogInputsQtd + _digitalInputsQtd); j++)
                    {
                        _inputs.Add(new InputClass(uid, j));
                    }
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
        public void Delete(Device device, string profileNameToActive)
        {
            _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;
            SetToActiveProfile(profileNameToActive);

            string sql = $"DELETE FROM dbo.profiles WHERE name = '{_name}' AND id_product = '{device.JJID}';";

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

        public void CreateNewProfileIntoObject(Device device, string profileName, bool setToActiveProfile = false)
        {
            try
            {
                _dbConnection = _dbConnection == null ? new DatabaseConnection() : _dbConnection;
                GetProfileIntoObject(profileName, device.JJID);

                // If not found a ID...
                if (string.IsNullOrEmpty(_id))
                {
                    string sql = $"INSERT INTO profiles (name, id_product) VALUES ('{profileName}', '{device.JJID}');";

                    if (!_dbConnection.RunSQL(sql))
                    {
                        // TODO: Create LOGFILE
                    }

                    sql = "SELECT IDENT_CURRENT('dbo.profiles') AS last_inserted_id";

                    using (JsonDocument json = _dbConnection.RunSQLWithResults(sql))
                    {
                        if (json != null)
                        {
                            GetProfileIntoObject(json.RootElement[0].GetProperty("last_inserted_id").GetString());
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

            using (JsonDocument json = database.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    for (int i = 0; i < json.RootElement.GetArrayLength(); i++)
                        list.Add(json.RootElement[i].GetProperty("name").ToString());
                }
            }

            return list;
        }
        #endregion
    }
}
