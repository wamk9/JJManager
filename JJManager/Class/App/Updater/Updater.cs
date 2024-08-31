using ArduinoUploader.Hardware;
using ArduinoUploader;
using JJManager.Class;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.SqlServer.Management.Smo;
using JJManager.Pages.App.Updater;

namespace JJManager.Class.App
{
    public class Updater : INotifyCollectionChanged
    {
        public enum UpdaterType
        {
            Device,
            Program,
            Plugin
        };

        protected String _ConnId = "";
        protected String _Name = "";
        protected String _DownloadURL = "";
        protected String _DownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JohnJohn3D", "JJManager", "downloads");
        protected String _DownloadFileName = "";
        protected UpdaterType _Type;
        protected Version _ActualVersion = null;
        protected Version _LastVersion = null;
        protected List<string[]> _ChangeLog = new List<string[]>();
        protected Main _MainForm = null;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public List<string[]> ChangeLog
        {
            get => _ChangeLog;
        }

        public Version LastVersion
        {
            get => _LastVersion;
        }

        public Version ActualVersion
        {
            get => _ActualVersion;
        }

        public String Name
        {
            get => _Name;
        }

        public String ConnId
        {
            get => _ConnId;
        }

        public UpdaterType Type
        {
            get => _Type; 
        }

        public bool NeedsUpdate
        {
            get => (_LastVersion > _ActualVersion ? true : false);
        }

        public static async Task<List<string>> GetUnavailableListEnties(ObservableCollection<Updater> updates)
        {
            List<Updater> pluginUpdaters = await GetPluginsUpdaterList();
            List<Updater> programUpdaters = await GetProgramUpdaterList();
            List<string> listToReturn = new List<string>();

            foreach (Updater updater in updates.Where(updater => updater.Type == UpdaterType.Plugin ))
            {
                if (!pluginUpdaters.Any(plugin => plugin.ConnId == updater.ConnId))
                {
                    listToReturn.Add(updater.ConnId);
                }
            }

            foreach (Updater updater in updates.Where(updater => { return updater.Type == UpdaterType.Program; }))
            {
                if (!programUpdaters.Any(program => program.ConnId == updater.ConnId))
                {
                    listToReturn.Add(updater.ConnId);
                }
            }

            return listToReturn;
        }

        public static async Task<List<Updater>> GetAvailableListEntries(ObservableCollection<Updater> updates)
        {
            List<Updater> pluginUpdaters = await GetPluginsUpdaterList();
            List<Updater> programUpdaters = await GetProgramUpdaterList();
            List<Updater> listToReturn = new List<Updater>();

            pluginUpdaters.RemoveAll(plugin =>
            {
                return (updates.Any(updater =>
                {
                    return (plugin.ConnId == updater.ConnId && plugin.ActualVersion == updater.ActualVersion && plugin.LastVersion == updater.LastVersion);
                }));
            });

            programUpdaters.RemoveAll(program =>
            {
                return (updates.Any(updater =>
                {
                    return (program.ConnId == updater.ConnId && program.ActualVersion == updater.ActualVersion && program.LastVersion == updater.LastVersion);
                }));
            });

            listToReturn.AddRange(pluginUpdaters);
            listToReturn.AddRange(programUpdaters);

            return listToReturn;
        }

        public static async Task<List<Updater>> GetProgramUpdaterList()
        {
            List<Updater> softwareUpdaters = new List<Updater>();
            List<string> programsName = new List<string>
            {
                Assembly.GetEntryAssembly().GetName().Name
            };

            List<Task> tasks = new List<Task>();

            foreach (var programName in programsName)
            {
                tasks.Add(Task.Run(() =>
                {
                    JJManager.Class.App.SoftwareUpdater softwareUpdater = new JJManager.Class.App.SoftwareUpdater(programName);

                    if (softwareUpdater.ActualVersion != null)
                    {
                        lock (softwareUpdater)
                        {
                            softwareUpdaters.Add(softwareUpdater);
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            return softwareUpdaters;
        }

        public static async Task<List<Updater>> GetPluginsUpdaterList()
        {
            List<Updater> pluginUpdaters = new List<Updater>();
            List<string> pluginsName = new List<string>
            {
                "JJManager Sync (Integração SimHub)"
            };

            List<Task> tasks = new List<Task>();

            foreach (var pluginName in pluginsName)
            {
                tasks.Add(Task.Run(() =>
                {
                    JJManager.Class.App.PluginUpdater pluginUpdater = new JJManager.Class.App.PluginUpdater(pluginName);

                    lock (pluginUpdater)
                    {
                        if (pluginUpdater.InExecution)
                        {
                            pluginUpdaters.Add(pluginUpdater);
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            return pluginUpdaters;
        }

        public static void UpdateList(ref ObservableCollection<Updater> updates, Updater newUpdate)
        {
            foreach (Updater update in updates.Intersect(new List<Updater> { newUpdate }, new UpdaterEqualityComparer()))
            {
                updates.Remove(update);
            }

            updates.Add(newUpdate);
        }

        public void Update(Main mainForm)
        {
            _MainForm = mainForm;

            using (WebClient wc = new WebClient())
            {
                if (!Directory.Exists(_DownloadPath))
                    Directory.CreateDirectory(_DownloadPath);

                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
                wc.DownloadFileAsync(new Uri(_DownloadURL), Path.Combine(_DownloadPath, _DownloadFileName));
            }
        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.ToString());
            }
            else if (e.Cancelled)
            {
                MessageBox.Show("Download do firmware cancelado.");
            }
            else
            {
                _MainForm.Invoke((MethodInvoker)delegate
                {
                    _MainForm.txtStatusUpdate.Text = "Atualizando '" + _Name + "'...";
                });

                try
                {
                    switch (_Type)
                    {
                        case UpdaterType.Plugin:
                            System.Diagnostics.Process pluginInstall = System.Diagnostics.Process.Start(Path.Combine(_DownloadPath, _DownloadFileName));
                            pluginInstall.Exited += Installer_Exited;
                            break;
                        case UpdaterType.Device:
                            JJManager.Class.App.DeviceUpdater deviceUpdater = this as JJManager.Class.App.DeviceUpdater;
                            string selectedComPort = null;

                            if (string.IsNullOrEmpty(deviceUpdater.ComPort))
                            {
                                IReadOnlyList<string> ports = deviceUpdater.Device.GetComPortByVidPidAndProductName();

                                if (ports.Count() == 0)
                                {
                                    Log.Insert("Updater", $"Não foi possível buscar as portas de comunicação do dispositivo '{deviceUpdater.Device.ProductName}' de ID '{deviceUpdater.Device.ConnId}'");
                                }
                                else if (ports.Count() == 1)
                                {
                                    selectedComPort = ports.First();
                                }
                                else // Multiple devices with same VID/PID
                                {
                                    _MainForm.Invoke((MethodInvoker)delegate
                                    {
                                        MultipleComPort multipleComPort = new MultipleComPort(ports);

                                        DialogResult dialogResult = multipleComPort.ShowDialog();

                                        if (dialogResult == DialogResult.OK)
                                        {
                                            selectedComPort = multipleComPort.Port;
                                        }
                                    });
                                }
                            }
                            ArduinoSketchUploaderOptions options = new ArduinoSketchUploaderOptions();
                            options.FileName = Path.Combine(_DownloadPath, _DownloadFileName);
                            options.PortName = !string.IsNullOrEmpty(deviceUpdater.ComPort) ? deviceUpdater.ComPort : selectedComPort;
                            options.ArduinoModel = ArduinoModel.Micro;

                            ArduinoSketchUploader uploader = new ArduinoSketchUploader(options);

                            uploader.UploadSketch();

                            _MainForm.Invoke((MethodInvoker)delegate
                            {
                                MessageBox.Show("Atualização de '" + _Name + "' realizada com sucesso!");
                                _MainForm.Invoke((MethodInvoker)delegate
                                {
                                    _MainForm.EnableAllForms();
                                    _MainForm.UpdateUpdaterList();
                                    _MainForm.txtStatusUpdate.Text = "";
                                });
                            });
                            break;
                        case UpdaterType.Program:
                            System.Diagnostics.Process programInstall = System.Diagnostics.Process.Start(Path.Combine(_DownloadPath, _DownloadFileName));
                            programInstall.Exited += Installer_Exited;

                            // Close JJManager if is that how will be updated.
                            if (_Name == Assembly.GetEntryAssembly().GetName().Name)
                            {
                                Environment.Exit(0);
                            }
                            break;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Insert("Firmware", "Ocorreu um problema ao atualizar " + _Name + ".", ex);
                    MessageBox.Show("Ocorreu um problema ao atualizar " + _Name + ", verifique se nenhum dispositivo está utilizando as portas COM no seu computador.");
                    
                    _MainForm.Invoke((MethodInvoker)delegate
                    {
                        _MainForm.EnableAllForms();
                        _MainForm.UpdateUpdaterList();
                        _MainForm.txtStatusUpdate.Text = "";
                    });
                }
                catch (Exception ex)
                {
                    Log.Insert("Firmware", "Ocorreu um problema ao atualizar " + _Name + ".", ex);
                    MessageBox.Show("Ocorreu um problema ao atualizar " + _Name + ", verifique se nenhum dispositivo está utilizando as portas COM no seu computador.");

                    _MainForm.Invoke((MethodInvoker)delegate
                    {
                        _MainForm.EnableAllForms();
                        _MainForm.UpdateUpdaterList();
                        _MainForm.txtStatusUpdate.Text = "";
                    });
                }
            }
        }

        private void Installer_Exited(object sender, EventArgs e)
        {
            File.Delete(Path.Combine(_DownloadPath, _DownloadFileName));

            _MainForm.Invoke((MethodInvoker)delegate
            {
                _MainForm.EnableAllForms();
                _MainForm.UpdateUpdaterList();
                _MainForm.txtStatusUpdate.Text = "";
            });
        }
    }

    public class UpdaterEqualityComparer : IEqualityComparer<Updater>
    {
        public bool Equals(Updater x, Updater y)
        {
            return x.ConnId == y.ConnId;
        }

        public int GetHashCode(Updater obj)
        {
            return obj.ConnId.GetHashCode();
        }
    }
}
