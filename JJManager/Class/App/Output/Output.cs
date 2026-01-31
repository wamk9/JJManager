using System;
using System.Text.Json.Nodes;
using LedsOutput = JJManager.Class.App.Output.Leds.Leds;
namespace JJManager.Class.App.Output
{
    public class Output
    {
        public enum OutputMode
        {
            None,
            Leds
        }

        public enum OutputType
        {
            None,
            Digital,
            Analog
        }

        protected uint _id = 0;
        protected uint _profileId = 0;
        protected string _name = null;
        protected JsonObject _data = null;
        protected OutputMode _mode = OutputMode.None;
        protected OutputType _type = OutputType.None;
        protected bool _changed = false;
        protected LedsOutput _led = null;
        //protected AudioControllerOutput _audioController = null;
        //protected AudioPlayerOutput _audioPlayer = null;


        public OutputMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        public OutputType Type
        {
            get => _type;
            set => _type = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }
        public uint Id
        {
            get => _id;
        }

        public bool Changed { get => _changed; set => _changed = value; }

        public LedsOutput Led { get => _led; set => _led = value; }

        //public MacroKeyOutput MacroKey
        //{
        //    get => _macroKey;
        //}

        //public AudioControllerOutput AudioController
        //{
        //    get => _audioController;
        //}

        //public AudioPlayerOutput AudioPlayer
        //{
        //    get => _audioPlayer;
        //}

        public JsonObject Data
        {
            get => _data;
            set => _data = value;
        }

        public void DataToObject()
        {
            switch (_mode)
            {
                case OutputMode.Leds:
                    //_leds = Leds.JsonToLeds(Data);
                    break;
            }
        }

        protected OutputMode ToOutputMode(string value)
        {
            switch (value)
            {
                case "leds":
                    return OutputMode.Leds;
                //case "audiocontroller":
                //    return OutputMode.AudioController;
                //case "audioplayer":
                //    return OutputMode.AudioPlayer;
            }

            return OutputMode.None;
        }

        protected OutputType ToOutputType(string value)
        {
            switch (value)
            {
                case "digital":
                    return OutputType.Digital;
                case "analog":
                    return OutputType.Analog;
            }

            return OutputType.None;
        }

        public Output(uint profileId, uint inputId, bool reset = false)
        {
            _id = inputId;
            _profileId = profileId;
            _data = new JsonObject();
            _name = $"Output {(_id + 1)}";

            RestartOutput(reset);
        }

        public void RestartOutput(bool reset)
        {
            DatabaseConnection database = new DatabaseConnection();
            String sql = $"SELECT name, mode, type, data FROM device_outputs WHERE id = '{_id}' AND id_profile = '{_profileId}';";


            foreach (JsonObject json in database.RunSQLWithResults(sql))
            {
                _data = json.ContainsKey("data") ? JsonObject.Parse(json["data"].GetValue<string>()).AsObject() : new JsonObject();
                _name = json.ContainsKey("name") && json["name"].GetValue<string>() != string.Empty ? json["name"].GetValue<string>() : $"Output {_id}";
                _mode = ToOutputMode(json.ContainsKey("mode") ? json["mode"].GetValue<string>() : "none");
                _type = ToOutputType(json.ContainsKey("type") ? json["type"].GetValue<string>() : "none");

                if (_mode == OutputMode.Leds)
                {
                    
                    _led = new LedsOutput(_data?["led"]?.AsObject() ?? new JsonObject());
                }
                //else if (_mode == OutputMode.AudioController)
                //{
                //    _audioController = new AudioControllerOutput(_data);
                //}
                //else if (_mode == OutputMode.AudioPlayer)
                //{
                //    _audioPlayer = new AudioPlayerOutput(_profileId, _id);
                //}   
            }
        }

        public bool Save()
        {
            if (!_changed)
            {
                return false;
            }

            if (_mode == OutputMode.Leds)
            {
                _data["led"] = _led.objectToJson();
                _name = _led.PropertyName;
            }

            string typeString = _type.ToString().ToLower();
            string modeString = _mode.ToString().ToLower();
            string dataString = _data.ToJsonString();

            string sql = $@"
                MERGE device_outputs WITH (SERIALIZABLE) AS T
                USING (VALUES ({_id}, '{_name}', '{typeString}', '{modeString}', '{dataString}', {_profileId})) AS U (id, name, type, mode, data, id_profile)
                ON U.id = T.id AND U.id_profile = T.id_profile
                WHEN MATCHED THEN
                    UPDATE SET name = '{_name}', type = '{typeString}', mode = '{modeString}', data = '{dataString}'
                WHEN NOT MATCHED THEN
                    INSERT (id, name, type, mode, data, id_profile) VALUES ({_id}, '{_name}', '{typeString}', '{modeString}', '{dataString}', {_profileId});
            ";

            if (!(new DatabaseConnection()).RunSQL(sql))
            {
                Log.Insert("Profile", "Ocorreu um erro ao salvar o output '" + _name + "' no banco de dados:\nSQL: " + sql);
                return false;
            }

            _changed = false;

            return true;
        }

        public void Exclude()
        {
            
            _mode = OutputMode.None;
            _data = new JsonObject();
            _led = null;
            Save();
        }

        private void DestroyAllExcept(OutputMode outputMode)
        {
            if (outputMode != OutputMode.Leds)
            {
                _led = null;
            }
            //if (inputMode != OutputMode.AudioController)
            //{
            //    _audioController = null;
            //}
            //if (inputMode != OutputMode.AudioPlayer)
            //{
            //    _audioPlayer = null;
            //}
        }

        public void RemoveFunction()
        {
            DestroyAllExcept(OutputMode.None);

            _mode = OutputMode.None;
            _type = OutputType.None;
        }


        public void SetToLeds(uint id, LedsOutput.LedType ledType)
        {
            DestroyAllExcept(OutputMode.Leds);

            if (_led == null)
            {
               _led = new LedsOutput(ledType);
            }

            _mode = OutputMode.Leds;
            _type = OutputType.Analog;
            _id = id;
        }

        public void Execute(JsonObject jsonData = null)
        {
            switch (_mode)
            {
                case OutputMode.Leds:
                    //_leds.Execute();
                    break;
                //case OutputMode.AudioController:
                //    //if (jsonData != null && jsonData.ContainsKey("value"))
                //    //{
                //    //    AudioController.SettedVolume = jsonData["value"].GetValue<int>();
                //    //}

                //    //Task.Run (async () => await AudioController.ChangeVolume()).Wait(1000);
                //    break;
                //case OutputMode.AudioPlayer:
                //    //_audioPlayer.PlayAudio();
                //    break;
            }
        }
    }
}
