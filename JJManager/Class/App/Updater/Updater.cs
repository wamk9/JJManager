using ArduinoUploader.Hardware;
using ArduinoUploader;
using JJManager.Class;
using JJManager.Class.Devices.Connections;
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

        public static async Task<List<string>> GetUnavailableListEnties(ObservableCollection<Updater> updates, ObservableCollection<Devices.JJDevice> devicesList = null)
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

            // Check for devices that are no longer available (disconnected from computer)
            if (devicesList != null)
            {
                foreach (Updater updater in updates.Where(updater => updater.Type == UpdaterType.Device))
                {
                    DeviceUpdater deviceUpdater = updater as DeviceUpdater;
                    if (deviceUpdater != null)
                    {
                        // Remove from updater list if device is no longer in the devices list
                        if (!devicesList.Any(device => device.ConnId == deviceUpdater.ConnId))
                        {
                            listToReturn.Add(updater.ConnId);
                        }
                    }
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

                wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);

                // Mostrar barra de progresso
                _MainForm.Invoke((MethodInvoker)delegate
                {
                    _MainForm.progressBarDownload.Visible = true;
                    _MainForm.progressBarDownload.Value = 0;
                });

                wc.DownloadFileAsync(new Uri(_DownloadURL), Path.Combine(_DownloadPath, _DownloadFileName));
            }
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _MainForm.Invoke((MethodInvoker)delegate
            {
                _MainForm.progressBarDownload.Value = e.ProgressPercentage;
                _MainForm.txtStatusUpdate.Text = $"Baixando '{_Name}'... {e.ProgressPercentage}%";
            });
        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // Esconder barra de progresso
            _MainForm.Invoke((MethodInvoker)delegate
            {
                _MainForm.progressBarDownload.Visible = false;
                _MainForm.progressBarDownload.Value = 0;
            });

            if (e.Error != null)
            {
                Pages.App.MessageBox.Show(_MainForm, "Erro no Download", $"Ocorreu um erro durante o download: {e.Error.Message}");
                _MainForm.Invoke((MethodInvoker)delegate
                {
                    _MainForm.txtStatusUpdate.Text = "";
                });
                return;
            }
            else if (e.Cancelled)
            {
                Pages.App.MessageBox.Show(_MainForm, "Download Cancelado", "Download do firmware cancelado.");
                _MainForm.Invoke((MethodInvoker)delegate
                {
                    _MainForm.txtStatusUpdate.Text = "";
                });
                return;
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

                            if (deviceUpdater.ComPort.Count == 0)
                            {
                                Log.Insert("Updater", $"Não foi possível buscar as portas de comunicação do dispositivo '{deviceUpdater.Device.ProductName}' de ID '{deviceUpdater.Device.ConnId}'");
                            }
                            else if (deviceUpdater.ComPort.Count == 1)
                            {
                                selectedComPort = deviceUpdater.ComPort.First();
                            }
                            else // Multiple devices with same VID/PID
                            {
                                _MainForm.Invoke((MethodInvoker)delegate
                                {
                                    MultipleComPort multipleComPort = new MultipleComPort(deviceUpdater.ComPort);

                                    DialogResult dialogResult = multipleComPort.ShowDialog();

                                    if (dialogResult == DialogResult.OK)
                                    {
                                        selectedComPort = multipleComPort.Port;
                                    }
                                });
                                
                            }
                            ArduinoSketchUploaderOptions options = new ArduinoSketchUploaderOptions();
                            options.FileName = Path.Combine(_DownloadPath, _DownloadFileName);
                            options.PortName = selectedComPort;
                            options.ArduinoModel = ArduinoModel.Micro;

                            // Mostrar barra de progresso
                            _MainForm.Invoke((MethodInvoker)delegate
                            {
                                _MainForm.progressBarDownload.Visible = true;
                                _MainForm.progressBarDownload.Value = 0;
                            });

                            // Criar Progress para acompanhar o upload
                            var progress = new Progress<double>(p =>
                            {
                                int percentage = (int)(p * 100);
                                _MainForm.Invoke((MethodInvoker)delegate
                                {
                                    _MainForm.progressBarDownload.Value = percentage;
                                    _MainForm.txtStatusUpdate.Text = $"Atualizando firmware de '{_Name}'... {percentage}%";
                                });
                            });

                            ArduinoSketchUploader uploader = new ArduinoSketchUploader(options, null, progress);

                            uploader.UploadSketch();

                            // Esconder barra de progresso
                            _MainForm.Invoke((MethodInvoker)delegate
                            {
                                _MainForm.progressBarDownload.Visible = false;
                                _MainForm.progressBarDownload.Value = 0;
                            });

                            _MainForm.Invoke((MethodInvoker)delegate
                            {
                                Pages.App.MessageBox.Show(_MainForm, "Atualização Concluída", "Atualização de '" + _Name + "' realizada com sucesso!");
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
                    Pages.App.MessageBox.Show(_MainForm, "Erro na Atualização", "Ocorreu um problema ao atualizar " + _Name + ", verifique se nenhum dispositivo está utilizando as portas COM no seu computador.");

                    _MainForm.Invoke((MethodInvoker)delegate
                    {
                        _MainForm.progressBarDownload.Visible = false;
                        _MainForm.progressBarDownload.Value = 0;
                        _MainForm.EnableAllForms();
                        _MainForm.UpdateUpdaterList();
                        _MainForm.txtStatusUpdate.Text = "";
                    });
                }
                catch (Exception ex)
                {
                    Log.Insert("Firmware", "Ocorreu um problema ao atualizar " + _Name + ".", ex);
                    Pages.App.MessageBox.Show(_MainForm, "Erro na Atualização", "Ocorreu um problema ao atualizar " + _Name + ", verifique se nenhum dispositivo está utilizando as portas COM no seu computador.");

                    _MainForm.Invoke((MethodInvoker)delegate
                    {
                        _MainForm.progressBarDownload.Visible = false;
                        _MainForm.progressBarDownload.Value = 0;
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
