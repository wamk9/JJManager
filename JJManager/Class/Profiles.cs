using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JJManager.Class
{
    public class Profiles
    {
        private String _id = "";
        private String _name = "";
        private String _idProduct = "";
        private Inputs[] _inputs = null;

        public String Id { get => _id; }
        public String IdProduct { get => _idProduct; }

        public Profiles(String name, String idProduct, int qtdInputs)
        {
            DatabaseConnection database = new DatabaseConnection();
            Inputs tmpInput = null;

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

            _inputs = new Inputs[qtdInputs];

            for (int j = 0; j < qtdInputs; j++)
            {
                tmpInput = new Inputs(_id, j.ToString());
                _inputs[j] = tmpInput;
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
            _inputs = null;
        }

        public Inputs GetInputByIndex(int index)
        {
            return _inputs[index];
        }

        public Inputs GetInputById(int idInput)
        {
            if (_inputs != null)
            { 
                foreach (Inputs input in _inputs)
                {
                    if (input.Id == idInput.ToString())
                        return input;
                }
            }

            Inputs newInput = new Inputs(_id, idInput.ToString());

            // InitProfile nessa situação é utilizado para repor a lista de inputs na ordem de indexação.
            //InitProfile();
            UpdateInputs();

            return newInput;
        }

        public void UpdateInputs()
        {
            Inputs tmpInput = null;

            for (int j = 1; j < _inputs.Length; j++)
            {
                tmpInput = new Inputs(_id, j.ToString());
                _inputs[(j - 1)] = tmpInput;
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
    }
}
