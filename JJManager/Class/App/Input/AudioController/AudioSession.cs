using AudioSwitcher.AudioApi.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJManager.Class.App.Input.AudioController
{
    public class AudioSession
    {
        public string Executable {  get; set; }
        public List<IAudioSession> Sessions { get; set; }

        public AudioSession (string executable)
        {
            Executable = executable;
            Sessions = new List<IAudioSession> ();
        }

        public void Add(IAudioSession session)
        {
            Sessions.Add (session);
        }

        public void Clear()
        {
            Sessions.Clear();
        }
    }
}
