using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager.Class
{
    internal class Log
    {
        public static void Insert (string module, string message, Exception exception = null) 
        {
            try
            {
                string _logFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JohnJohn3D", "JJManager", "Log");
                string _logFilePath = Path.Combine(_logFolderPath, "Log_" + module + ".txt");

                if (!Directory.Exists(_logFolderPath))
                    Directory.CreateDirectory(_logFolderPath);


                if (!File.Exists(_logFilePath))
                {
                    File.Create(_logFilePath).Close();
                }

                string text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + message + Environment.NewLine;
                
                if (exception != null)
                {
                    text += "Exception path: " + exception.Source + Environment.NewLine;
                    text += "Exception method: " + exception.TargetSite + Environment.NewLine;
                    text += "Exception message: " + exception.Message + Environment.NewLine;
                    text += "Exception trace: " + exception.StackTrace + Environment.NewLine;
                }

                text += "-----------------------------------------------" + Environment.NewLine;
                
                File.AppendAllText(_logFilePath, text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Falha ao criar arquivo de log:\r\n" + ex.Message);
            }
        }
    }
}
