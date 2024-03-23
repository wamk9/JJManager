using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JJManager.Class
{
    public class AnalogInput
    {
        private String _id = "";
        private String _name = "";
        private String _type = "";
        private String _info = "";
        private bool _invertedAxis = false;
        private String _idProfile = "";
        private String _style = "";
        private JJManager.Class.App.AudioManager _audioManager = null;
        private int _settedVolume = -1;
        private bool _audioCoreNeedsRestart = true;

        public String Name { get { return _name; } }
        public String Type { get { return _type; } }
        public String Info { get { return _info; } }
        public String Id { get { return _id; } }
        public bool InvertedAxis { get { return _invertedAxis; } }
        public String IdProfile { get { return _idProfile; } }
        public JJManager.Class.App.AudioManager AudioManager { get { return _audioManager; } }
        public bool AudioCoreNeedsRestart { set { _audioCoreNeedsRestart = value; } get { return _audioCoreNeedsRestart; } }

        public AnalogInput(String profileId, String inputId)
        {
            DatabaseConnection database = new DatabaseConnection();
            String sql = "SELECT name, type, info, inverted_axis FROM analog_inputs WHERE id = '" + inputId + "' AND id_profile = '" + profileId + "';";

            using (JsonDocument json = database.RunSQLWithResults(sql)) 
            {
                _id = inputId;
                _idProfile = profileId;

                if (json != null)
                {
                    _name = json.RootElement[0].GetProperty("name").ToString();
                    _type = json.RootElement[0].GetProperty("type").ToString();
                    _info = json.RootElement[0].GetProperty("info").ToString();
                    _invertedAxis = bool.Parse(json.RootElement[0].GetProperty("inverted_axis").ToString());

                    //if (_type == "audio")
                    _audioManager = new JJManager.Class.App.AudioManager();
                } 
                else
                {
                    SaveInputData("Input " + inputId);
                }
            }
        }

        public bool SaveInputData(String name, String type = "nothing", String info = "", bool invertedAxis = false)
        {
            DatabaseConnection database = new DatabaseConnection();
            
            String sql = "MERGE analog_inputs WITH (SERIALIZABLE) AS T USING (VALUES (" + _id + ",'" + @name + "', '" + type + "', '" + @info + "', " + (invertedAxis ? 1 : 0) + ", " + _idProfile + ")) AS U (id, name, type, info, inverted_axis, id_profile) " +
                            "ON U.id = T.id AND U.id_profile = T.id_profile WHEN MATCHED THEN " +
                                "UPDATE SET name='" + @name + "', type='" + type + "', info='" + @info + "', inverted_axis=" + (invertedAxis ? 1 : 0) + " " +
                            "WHEN NOT MATCHED THEN " +
                            "INSERT (id, name, type, info, inverted_axis, id_profile) VALUES (" + _id + ", '" + @name + "', '" + type + "', '" + @info + "', " + (invertedAxis ? 1 : 0) + " , " + _idProfile + ");";

            if (!database.RunSQL(sql))
            {
                // TODO: Create LOGFILE
                return false;
            }

            _name = @name;
            _type = type;
            _info = @info;
            _invertedAxis = invertedAxis;

            return true;
        }

        public void ManageAudioVolume(int settedVolume, out bool audioCoreRestarted)
        {
            /*if (_settedVolume == settedVolume)
            {
                return;
            }*/

            _settedVolume = settedVolume;
            _audioManager.ChangeVolume(this, _settedVolume, out audioCoreRestarted);
        }
    }
}
