using AudioSwitcher.AudioApi.CoreAudio;
using HidSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace JJManager.Class
{
    public class Profile
    {
        private String _id = "";
        private String _name = "";
        private String _idProduct = "";
        private int _AnalogInputsQtd = 0;
        private int _DigitalInputsQtd = 0;
        private AnalogInput[] _analogInputs = null;


        public String Id { get => _id; }
        public String IdProduct { get => _idProduct; }
        public String Name { get => _name; }

        public Profile(string id)
        {
            DatabaseConnection database = new DatabaseConnection();
            AnalogInput tmpInput = null;

            String sql = "SELECT p.name, jjp.analog_inputs_qtd, jjp.digital_inputs_qtd FROM profiles AS p LEFT JOIN jj_products AS jjp ON (p.id_product = jjp.id) WHERE p.id = '" + id + "';";

            using (JsonDocument json = database.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    _id = id;
                    _name = json.RootElement[0].GetProperty("name").GetString();
                    _AnalogInputsQtd = Int32.Parse(json.RootElement[0].GetProperty("analog_inputs_qtd").GetString());
                    _DigitalInputsQtd = Int32.Parse(json.RootElement[0].GetProperty("digital_inputs_qtd").GetString());
                }
            }

            _analogInputs = new AnalogInput[_AnalogInputsQtd];

            for (int j = 0; j < _AnalogInputsQtd; j++)
            {
                tmpInput = new AnalogInput(_id, (j + 1).ToString());
                _analogInputs[j] = tmpInput;
            }
        }

        public Profile(String name, String idProduct, int qtdInputs)
        {
            DatabaseConnection database = new DatabaseConnection();
            AnalogInput tmpInput = null;

            String sql = "SELECT id FROM profiles WHERE name = '" + name + "' AND id_product = '" + idProduct + "';";

            using (JsonDocument json = database.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    _id = json.RootElement[0].GetProperty("id").ToString();
                    _idProduct = idProduct;
                    _name = name;
                }
            }

            if (_id == "")
            {
                sql = "INSERT INTO profiles (name, id_product) VALUES ('" + name + "', " + idProduct + ");";

                if (!database.RunSQL(sql))
                {
                    // TODO: Create LOGFILE
                }

                sql = "SELECT TOP 1 id FROM profiles ORDER BY id desc;";

                using (JsonDocument json = database.RunSQLWithResults(sql))
                {
                    if (json != null)
                    {
                        _id = json.RootElement[0].GetProperty("id").ToString();
                        _idProduct = idProduct;
                        _name = name;
                    }
                }
            }

            _analogInputs = new AnalogInput[qtdInputs];

            for (int j = 0; j < qtdInputs; j++)
            {
                tmpInput = new AnalogInput(_id, (j + 1).ToString());
                _analogInputs[j] = tmpInput;
            }
        }

        public void Delete(String profileName, String productId)
        {
            DatabaseConnection database = new DatabaseConnection();

            String sql = "DELETE FROM dbo.profiles WHERE name = '" + profileName + "' AND id_product = " + productId + ";";

            if (!database.RunSQL(sql))
            {
                // TODO: Create LOGFILE
            }

            _id = "";
            _name = "";
            _idProduct = "";
            _analogInputs = null;
        }

        public AnalogInput[] getAllInputs()
        {
            return _analogInputs;
        }

        public AnalogInput GetInputByIndex(int index)
        {
            return _analogInputs[index];
        }

        public AnalogInput GetAnalogInputById(int idInput)
        {
            if (_analogInputs != null)
            { 
                foreach (AnalogInput input in _analogInputs)
                {
                    if (input.Id == idInput.ToString())
                        return input;
                }
            }

            AnalogInput newInput = new AnalogInput(_id, idInput.ToString());

            // InitProfile nessa situação é utilizado para repor a lista de inputs na ordem de indexação.
            //InitProfile();
            UpdateInputs();

            return newInput;
        }

        public void UpdateInputs()
        {
            AnalogInput tmpInput = null;

            for (int j = 1; j < _analogInputs.Length; j++)
            {
                tmpInput = new AnalogInput(_id, j.ToString());
                _analogInputs[(j - 1)] = tmpInput;
            }
        }

        public static List<String> GetList(String productId)
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

                return list;
            }
        }

        public static Profile CreateTo(Device device, string profileName)
        {
            DatabaseConnection database = new DatabaseConnection();
            string sql = "INSERT INTO dbo.profiles (name, id_product) VALUES ('" + profileName + "', '" + device.JJID + "');";
            Profile profile = null;

            if (!database.RunSQL(sql))
            {
                Log.Insert("Profiles", "Ocorreu um problema ao inserir o profile (" + sql + ").");
            }

            sql = "SELECT IDENT_CURRENT('dbo.profiles') AS last_inserted_id";

            using (JsonDocument json = database.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    profile = new Profile(json.RootElement[0].GetProperty("last_inserted_id").GetInt32().ToString());
                }
            }

            return profile;
        }


        public static Profile CreateTo(string connId, string profileName)
        {
            DatabaseConnection database = new DatabaseConnection();
            string sql = "INSERT INTO dbo.profiles (name, id_product) VALUES ('" + profileName + "', '" + connId + "');";
            Profile profile = null;

            if (!database.RunSQL(sql))
            {
                Log.Insert("Profiles", "Ocorreu um problema ao inserir o profile (" + sql + ").");
            }

            sql = "SELECT IDENT_CURRENT('dbo.profiles') AS last_inserted_id";

            using (JsonDocument json = database.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    profile = new Profile(json.RootElement[0].GetProperty("last_inserted_id").GetInt32().ToString());
                }
            }

            return profile;
        }

        public static Profile getProfile(string productId, string profileName)
        {
            Profile profile = null;
            DatabaseConnection database = new DatabaseConnection();

            string sql = "SELECT p.id FROM dbo.profiles AS p WHERE p.name = '" + profileName + "' AND p.id_product = '" + productId + "'";

            using (JsonDocument json = database.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    profile = new Profile(json.RootElement[0].GetProperty("id").GetString());
                }
            }

            return profile;
        }

        public static Profile getActiveProfile(string connId)
        {
            DatabaseConnection database = new DatabaseConnection();
            Profile profile = null;

            String sql = "SELECT id_profile FROM dbo.user_products WHERE conn_id = '" + connId + "';";

            using (JsonDocument json = database.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    profile = new Profile(json.RootElement[0].GetProperty("id_profile").ToString());
                }
                else
                {

                    //Devices device = Devices.getDevice(connId);

                    profile = Profile.getProfile(Device.GetJJProductId(Device.getJJProductName(connId)), "Perfil Padrão");

                    if (profile == null)
                        profile = Profile.CreateTo(Device.GetJJProductId(Device.getJJProductName(connId)), "Perfil Padrão");

                    //profile = new Profiles(Devices.getDevice(hash));

                }
            }

            return profile;
        }

        public static Profile getProfile(Device device, string profileName)
        {
            Profile profile = null;
            DatabaseConnection database = new DatabaseConnection();

            string sql = "SELECT p.id FROM dbo.profiles AS p WHERE p.name = '" + profileName + "' AND p.id_product = '" + device.JJID + "'";

            using (JsonDocument json = database.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    profile = new Profile(json.RootElement[0].GetProperty("id").GetInt32().ToString());
                }
            }

            return profile;
        }

        public static Profile getActiveProfile(Device device)
        {
            DatabaseConnection database = new DatabaseConnection();
            Profile profile = null;

            String sql = "SELECT id_profile FROM dbo.user_products WHERE conn_id = '" + device.ConnId + "';";

            using (JsonDocument json = database.RunSQLWithResults(sql))
            {
                if (json != null)
                {
                    profile = new Profile(json.RootElement[0].GetProperty("id_profile").ToString());
                }
                else
                {

                    //Devices device = Devices.getDevice(connId);

                    profile = Profile.getProfile(device, "Perfil Padrão");

                    if (profile == null)
                        profile = Profile.CreateTo(device, "Perfil Padrão");

                    //profile = new Profiles(Devices.getDevice(hash));

                }
            }

            return profile;
        }

        public void SendInputsByHID(HidDevice hidDevice)
        {
            try
            {
                var reportDescriptor = hidDevice.GetReportDescriptor();
                String ReturnDictionary = "";
                DatabaseConnection _DatabaseConnection = new DatabaseConnection();

                foreach (var deviceItem in reportDescriptor.DeviceItems)
                {
                    HidStream hidStream;
                    if (hidDevice.TryOpen(out hidStream))
                    {
                        hidStream.ReadTimeout = Timeout.Infinite;

                        SortedDictionary<String, String> InputNames = _DatabaseConnection.GetAllInputName(_id);


                        String InputsString = "";

                        for (int i = 0; i < _AnalogInputsQtd; i++)
                        {
                            if (InputNames.TryGetValue((i + 1).ToString(), out ReturnDictionary))
                            {
                                if (i == 0)
                                {
                                    if (ReturnDictionary == "" || ReturnDictionary == null)
                                        InputsString = "Input " + (i + 1).ToString();
                                    else
                                        InputsString = ReturnDictionary;
                                }
                                else
                                {
                                    if (ReturnDictionary == "")
                                        InputsString += "|" + "Input " + (i + 1).ToString();
                                    else
                                        InputsString += "|" + ReturnDictionary;
                                }
                            }
                            else
                            {
                                if (i == 0)
                                {
                                    InputsString = "Input " + (i + 1).ToString();
                                }
                                else
                                {
                                    InputsString += "|" + "Input " + (i + 1).ToString();
                                }

                            }
                        }

                        //byte[] input = Encoding.ASCII.GetBytes("IPT1|Teste");
                        byte[] input = Encoding.ASCII.GetBytes(InputsString);
                        byte[] inputTreated = new byte[(input.Length + 1)];
                        for (int i = 0; i < inputTreated.Length; i++)
                        {
                            if (i == 0)
                                inputTreated[i] = 0x00;
                            else
                                inputTreated[i] = input[(i - 1)];
                        }

                        hidStream.Write(inputTreated);
                    }
                }
            }
            catch (IOException ex)
            {

            }
            catch (ThreadAbortException ex)
            {
                MessageBox.Show("Bugou");
            }
        }
    }
}
