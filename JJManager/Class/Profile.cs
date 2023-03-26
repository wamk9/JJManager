using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace JJManager.Class
{
    internal class Profile
    {
        private DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private String _Id = String.Empty;

        public String Id
        {
            get => _Id;
            set { _Id = value; }
        }

        public Profile ()
        {
            
        }
    }
}
