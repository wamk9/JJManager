using NAudio.CoreAudioApi;
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
        public List<AudioSessionControl> Sessions { get; set; }
        public List<int> PID { get; set; }

        public AudioSession (string executable)
        {
            Executable = executable;
            PID = new List<int>();
            Sessions = new List<AudioSessionControl> ();
        }

        public void Add(AudioSessionControl session)
        {
            Sessions.Add (session);
        }

        public void Add(int pid)
        {
            PID.Add(pid);
        }


        public void Clear()
        {
            PID.Clear();
            Sessions.Clear();
        }
    }
}
