using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JJManager.Class
{
    public class Log
    {
        private static string _logFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JohnJohn3D", "JJManager", "Log");

        private static string GetModuleName(string filePath)
        {
            string logRegex = @Path.Combine(_logFolderPath, "Log_(.*)").Replace("\\", "\\\\") + "\\.txt";

            Match match = Regex.Match(filePath, logRegex);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }


        public static IEnumerable<string[]> GetModulesInfo()
        {
            List<string[]> modules = new List<string[]>();

            if (!Directory.Exists(_logFolderPath))
                return modules;

            string[] files = Directory.GetFiles(_logFolderPath);
            string fileSize = null;
            double fileLength = 0;
            FileInfo fileInfo = null;

            foreach (string file in files)
            {
                fileInfo = new FileInfo(file);

                if (fileInfo.Exists)
                {
                    fileLength = fileInfo.Length;

                    if (fileLength < 1024)
                    {
                        fileSize = $"{fileLength} bytes";
                    }
                    else if (fileLength < 1024 * 1024)
                    {
                        fileSize = $"{(fileLength / 1024.0):F2} KB";
                    }
                    else
                    {
                        fileSize = $"{(fileLength / 1024.0 / 1024.0):F2} MB";
                    }
                }

                modules.Add(new string[]
                {
                    GetModuleName(file),
                    fileSize
                });

                fileLength = 0;
                fileSize = null;
            }

            return modules;
        }

        public static void CleanLog(string module)
        {
            try
            {
                string _logFilePath = Path.Combine(_logFolderPath, "Log_" + module + ".txt");

                if (File.Exists(_logFilePath))
                {
                    File.Delete(_logFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao tentar limpar os logs de '{module}': {ex.Message}");
            }
        }

        public static void CleanLogs()
        {
            try
            {
                string logRegex = @Path.Combine(_logFolderPath, "Log_(.*)") + "\\.txt";
                List<string> modulesUndeleted = new List<string>();

                if (Directory.Exists(_logFolderPath))
                {
                    foreach (string filePath in Directory.GetFiles(_logFolderPath))
                    {
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch (Exception exFile)
                        {
                            string moduleName = GetModuleName(filePath);
                            Log.Insert("Log", $"Falha ao deletar o log do módulo '{moduleName}'", exFile);
                            modulesUndeleted.Add(moduleName);
                        }
                    }

                    if (modulesUndeleted.Count > 0)
                    {
                        MessageBox.Show($"Não foi possível limpar os logs dos módulos: {string.Join(", ", modulesUndeleted)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Insert("Log", $"Falha ao deletar os logs do JJManager", ex);
                MessageBox.Show($"Ocorreu um erro ao tentar limpar os logs: {ex.Message}");
            }
        }

        public static void Insert (string module, string message, Exception exception = null) 
        {
            try
            {
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
            catch (Exception)
            {
                // Nobody needs know...
                //MessageBox.Show("Falha ao criar arquivo de log:\r\n" + ex.Message);
            }
        }

        public static void Open(string module)
        {
            try
            {
                string logFilePath = Path.Combine(_logFolderPath, "Log_" + module + ".txt");

                // Open the .txt file with the default application
                Process.Start(new ProcessStartInfo(logFilePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while trying to open the file: " + ex.Message);
            }
        }
    }
}
