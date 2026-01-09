using AudioSwitcher.AudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using MacroKeyInput = JJManager.Class.App.Input.MacroKey.MacroKey;
using AudioControllerInput = JJManager.Class.App.Input.AudioController.AudioController;
using AudioPlayerInput = JJManager.Class.App.Input.AudioPlayer.AudioPlayer;
using AudioSwitcher.AudioApi.CoreAudio;
using JJManager.Class.App.Profile;
using Newtonsoft.Json.Linq;
namespace JJManager.Class.App.Input
{
    public class Input
    {
        public enum InputMode
        {
            None,
            MacroKey,
            AudioController,
            AudioPlayer
        }

        public enum InputType
        {
            None,
            Digital,
            Analog
        }

        protected uint _id = 0;
        protected uint _profileId = 0;
        protected string _name = null;
        protected JsonObject _data = null;
        protected InputMode _mode = InputMode.None;
        protected InputType _type = InputType.None;

        protected MacroKeyInput _macroKey = null;
        protected AudioControllerInput _audioController = null;
        protected AudioPlayerInput _audioPlayer = null;


        public InputMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        public InputType Type
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

        public MacroKeyInput MacroKey
        {
            get => _macroKey;
        }

        public AudioControllerInput AudioController
        {
            get => _audioController;
        }

        public AudioPlayerInput AudioPlayer
        {
            get => _audioPlayer;
        }

        public JsonObject Data
        {
            get => _data;
            set => _data = value;
        }

        protected InputMode ToInputMode(string value)
        {
            switch (value)
            {
                case "macrokey":
                    return InputMode.MacroKey;
                case "audiocontroller":
                    return InputMode.AudioController;
                case "audioplayer":
                    return InputMode.AudioPlayer;
            }

            return InputMode.None;
        }

        protected InputType ToInputType(string value)
        {
            switch (value)
            {
                case "digital":
                    return InputType.Digital;
                case "analog":
                    return InputType.Analog;
            }

            return InputType.None;
        }

        public Input(uint profileId, uint inputId)
        {
            _id = inputId;
            _profileId = profileId;
            _data = new JsonObject();
            _name = $"Input {(_id + 1)}";

            RestartInput();
        }

        public void RestartInput()
        {
            DatabaseConnection database = new DatabaseConnection();
            String sql = $"SELECT name, mode, type, data FROM device_inputs WHERE id = '{_id}' AND id_profile = '{_profileId}';";


            foreach (JsonObject json in database.RunSQLWithResults(sql))
            {
                _data = json.ContainsKey("data") ? JsonObject.Parse(json["data"].GetValue<string>()).AsObject() : new JsonObject();
                _name = json.ContainsKey("name") && json["name"].GetValue<string>() != string.Empty ? json["name"].GetValue<string>() : $"Input {_id}";
                _mode = ToInputMode(json.ContainsKey("mode") ? json["mode"].GetValue<string>() : "none");
                _type = ToInputType(json.ContainsKey("type") ? json["type"].GetValue<string>() : "none");

                if (_mode == InputMode.MacroKey)
                {
                    _macroKey = new MacroKeyInput(_data);
                }
                else if (_mode == InputMode.AudioController)
                {
                    _audioController = new AudioControllerInput(_data);
                }
                else if (_mode == InputMode.AudioPlayer)
                {
                    _audioPlayer = new AudioPlayerInput(_profileId, _id);
                }
            }
        }

        public bool Save()
        {
            string typeString = _type.ToString().ToLower();
            string modeString = _mode.ToString().ToLower();
            string dataString = _data.ToJsonString();

            string sql = $@"
                MERGE device_inputs WITH (SERIALIZABLE) AS T
                USING (VALUES ({_id}, '{_name}', '{typeString}', '{modeString}', '{dataString}', {_profileId})) AS U (id, name, type, mode, data, id_profile)
                ON U.id = T.id AND U.id_profile = T.id_profile
                WHEN MATCHED THEN
                    UPDATE SET name = '{_name}', type = '{typeString}', mode = '{modeString}', data = '{dataString}'
                WHEN NOT MATCHED THEN
                    INSERT (id, name, type, mode, data, id_profile) VALUES ({_id}, '{_name}', '{typeString}', '{modeString}', '{dataString}', {_profileId});
            ";

            if (!(new DatabaseConnection()).RunSQL(sql))
            {
                Log.Insert("Profile", "Ocorreu um erro ao salvar o input digital '" + _name + "' no banco de dados:\nSQL: " + sql);
                return false;
            }

            return true;
        }

        private void DestroyAllExcept(InputMode inputMode)
        {
            if (inputMode != InputMode.MacroKey)
            {
                _macroKey = null;
            }
            if (inputMode != InputMode.AudioController)
            {
                _audioController = null;
            }
            if (inputMode != InputMode.AudioPlayer)
            {
                _audioPlayer = null;
            }
        }

        public void RemoveFunction()
        {
            DestroyAllExcept(InputMode.None);

            _mode = InputMode.None;
            _type = InputType.None;
        }


        public void SetToMacroKey()
        {
            DestroyAllExcept(InputMode.MacroKey);

            if (_macroKey == null)
            {
                _macroKey = new MacroKeyInput();
            }

            _mode = InputMode.MacroKey;
            _type = InputType.Digital;
        }

        public void SetToAudioController()
        {
            DestroyAllExcept(InputMode.AudioController);

            if (_audioController == null)
            {
                _audioController = new AudioControllerInput();
            }

            _mode = InputMode.AudioController;
            _type = InputType.Analog;
        }

        public void SetToAudioPlayer()
        {
            DestroyAllExcept(InputMode.AudioPlayer);

            if (_audioPlayer == null)
            {
                _audioPlayer = new AudioPlayerInput(_profileId, _id);
            }

            _mode = InputMode.AudioPlayer;
            _type = InputType.Digital;
        }

        public void Execute(JsonObject jsonData = null)
        {
            switch (_mode)
            {
                case InputMode.MacroKey:
                    _macroKey.Execute();
                    break;
                case InputMode.AudioController:
                    if (jsonData != null && jsonData.ContainsKey("value"))
                    {
                        AudioController.SettedVolume = jsonData["value"].GetValue<int>();
                    }

                    // Fire and forget - don't block or create unnecessary threads
                    // AudioController.ChangeVolume() handles its own async operations
                    _ = AudioController.ChangeVolume();
                    break;
                case InputMode.AudioPlayer:
                    _audioPlayer.PlayAudio();
                    break;
            }
        }
    }
}
