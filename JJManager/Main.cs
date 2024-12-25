using JJManager.Class;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ConfigClass = JJManager.Class.App.Config.Config;
using JJManager.Class.App.Fonts;
using System.Drawing.Drawing2D;
using JJDeviceClass = JJManager.Class.Devices.JJDevice;

namespace JJManager
{
    public partial class Main : MaterialForm
    {
        private ObservableCollection<JJDeviceClass> _DevicesList = new ObservableCollection<JJDeviceClass>();
        private ObservableCollection<JJManager.Class.App.Updater> _UpdaterList = new ObservableCollection<JJManager.Class.App.Updater>();
        //private List<JJManager.Class.Device> _BtDevicesList = new List<JJManager.Class.Device>();
        private Point _mousePosition = Point.Empty;

        public static int LvDevicesIndex = -1;
        public static int LvDevicesToUpdateIndex = -1;

        private MaterialSkinManager materialSkinManager = null;
        public Thread threadUpdateDevicesList = null;

        private AppModulesNotifyIcon notifyIcon = null;
        private AppModulesTimer fillListsTimer = null;

        #region Constructors
        public Main()
        {
            InitializeComponent();

            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            DisableAllForms();

            lblAboutVersion.Text = "JJManager Versão " + Assembly.GetEntryAssembly().GetName().Version.ToString();
            lblAboutText.Text = "Criado com o propósito de servir como um gerenciador para os produtos da série JJM, JJSD e JJB, o JJManager é uma solução completa que trás aos usuários diversas funções em seus produtos nos quais não são disponíveis de forma autônoma.";

            Migrate migrate = new Migrate();

            lvDevices.FullRowSelect = true;

            // Events
            FormClosing += new FormClosingEventHandler(Main_FormClosing);
            FormClosed += new FormClosedEventHandler(Main_FormClosed);

            // Start NotifyIcon
            notifyIcon = new AppModulesNotifyIcon(components, "Você continua com dispositivos conectados ao JJManager, para encerrar o programa você deve desconectar todos os dispositivos.", NotifyIcon_Click);
            fillListsTimer = new AppModulesTimer(components, 2000, FillListsTimer_tick);

            Shown += Main_Shown;

            _DevicesList.CollectionChanged += _DevicesList_CollectionChanged;
            _UpdaterList.CollectionChanged += _UpdaterList_CollectionChanged;

            LoadLogData();
        }
        #endregion

        #region Collections
        private void _DevicesList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems)
                {
                    JJDeviceClass device = newItem as JJDeviceClass;
                    device.PropertyChanged += Device_PropertyChanged;

                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            JJManager.Class.App.DeviceUpdater updater = new JJManager.Class.App.DeviceUpdater(this);
                            updater.CheckUpdate(device);
                            _UpdaterList.Add(updater);

                            lvDevices.Items.Add(new ListViewItem(new string[]
                            {
                                device.ConnId,
                                device.ProductName,
                                device.DeviceType.ToString(),
                                device.IsConnected ? "Conectado" : "Desconectado"
                            }));
                        });
                    }
                    else
                    {
                        JJManager.Class.App.DeviceUpdater updater = new JJManager.Class.App.DeviceUpdater(this);
                        updater.CheckUpdate(device);
                        _UpdaterList.Add(updater);

                        lvDevices.Items.Add(new ListViewItem(new string[]
                            {
                                device.ConnId,
                                device.ProductName,
                                device.DeviceType.ToString(),
                                device.IsConnected ? "Conectado" : "Desconectado"
                            }));
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            JJDeviceClass device = oldItem as JJDeviceClass;
                            device.PropertyChanged -= Device_PropertyChanged;

                            for (int i = 0; i < lvDevices.Items.Count; i++)
                            {
                                if (lvDevices.Items[i].SubItems[0].Text == device.ConnId)
                                {
                                    lvDevices.Items[i].Remove();
                                }
                            }
                        });
                    }
                    else
                    {
                        JJDeviceClass device = oldItem as JJDeviceClass;
                        device.PropertyChanged -= Device_PropertyChanged;

                        for (int i = 0; i < lvDevices.Items.Count; i++)
                        {
                            if (lvDevices.Items[i].SubItems[0].Text == device.ConnId)
                            {
                                lvDevices.Items[i].Remove();
                            }
                        }
                    }
                }
            }
        }

        private void _UpdaterList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems)
                {
                    JJManager.Class.App.Updater updater = newItem as JJManager.Class.App.Updater;
                    bool inOnList = false;

                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            for (int i = 0; i < lvDevicesToUpdate.Items.Count; i++)
                            {
                                if (lvDevicesToUpdate.Items[i].SubItems[0].Text == updater.ConnId)
                                {
                                    lvDevicesToUpdate.Items[i].SubItems[1].Text = updater.Name;
                                    lvDevicesToUpdate.Items[i].SubItems[2].Text = (updater.ActualVersion != null ? updater.ActualVersion.ToString() : "Não identif.");
                                    lvDevicesToUpdate.Items[i].SubItems[3].Text = (updater.LastVersion != null ? updater.LastVersion.ToString() : "N/A");

                                    inOnList = true;
                                }
                            }

                            if (!inOnList)
                            {
                                lvDevicesToUpdate.Items.Add(new ListViewItem(new string[]
                                {
                                    updater.ConnId,
                                    updater.Name,
                                    (updater.ActualVersion != null ? updater.ActualVersion.ToString() : "Não identif."),
                                    (updater.LastVersion != null ? updater.LastVersion.ToString() : "N/A")
                                }));
                            }
                        });
                    }
                    else
                    {
                        for (int i = 0; i < lvDevicesToUpdate.Items.Count; i++)
                        {
                            if (lvDevicesToUpdate.Items[i].SubItems[0].Text == updater.ConnId)
                            {
                                lvDevicesToUpdate.Items[i].SubItems[1].Text = updater.Name;
                                lvDevicesToUpdate.Items[i].SubItems[2].Text = (updater.ActualVersion != null ? updater.ActualVersion.ToString() : "Não identif.");
                                lvDevicesToUpdate.Items[i].SubItems[3].Text = (updater.LastVersion != null ? updater.LastVersion.ToString() : "N/A");

                                inOnList = true;
                            }
                        }

                        if (!inOnList)
                        {
                            lvDevicesToUpdate.Items.Add(new ListViewItem(new string[]
                            {
                                    updater.ConnId,
                                    updater.Name,
                                    (updater.ActualVersion != null ? updater.ActualVersion.ToString() : "Não identif."),
                                    (updater.LastVersion != null ? updater.LastVersion.ToString() : "N/A")
                            }));
                        }
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems)
                {
                    JJManager.Class.App.Updater updater = oldItem as JJManager.Class.App.Updater;

                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            for (int i = 0; i < lvDevicesToUpdate.Items.Count; i++)
                            {
                                if (lvDevicesToUpdate.Items[i].SubItems[0].Text ==  updater.ConnId)
                                {
                                    lvDevicesToUpdate.Items[i].Remove();
                                }
                            }
                        });
                    }
                    else
                    {
                        for (int i = 0; i < lvDevicesToUpdate.Items.Count; i++)
                        {
                            if (lvDevicesToUpdate.Items[i].SubItems[0].Text == updater.ConnId)
                            {
                                lvDevicesToUpdate.Items[i].Remove();
                            }
                        }
                    }
                }
            }
        }

        private void Device_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            JJDeviceClass device = sender as JJDeviceClass;

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    ChangeDeviceConnection(device.ConnId, device.IsConnected, true);
                });
            }
            else
            {
                ChangeDeviceConnection(device.ConnId, device.IsConnected, true);
            }

        }
        #endregion

        #region LoadAndManipulatingLists
        //private void CheckDevicesListBluetooth()
        //{
        //    _BtDevicesList = JJManager.Class.Device.getBtDevicesList(_BtDevicesList);

        //    if (lvDevices.Items.Count == 0 && _BtDevicesList.Count > 0 || lvDevices.Items.Count != (_DevicesList.Count + _BtDevicesList.Count))
        //    {
        //        //UpdateDevicesList();
        //    }
        //    else if (lvDevices.Items.Count == (_DevicesList.Count + _BtDevicesList.Count))
        //    {
        //        foreach (ListViewItem actualDeviceInList in lvDevices.Items)
        //        {
        //            if (actualDeviceInList.SubItems[2].Text == "Bluetooth" &&
        //                (!_BtDevicesList.Exists(newDevice => newDevice.ConnId == actualDeviceInList.SubItems[0].Text && newDevice.IsConnected == (actualDeviceInList.SubItems[0].Text == "Conectado" ? true : false))))
        //            {
        //                //UpdateDevicesList();
        //            }
        //        }
        //    }
        //}

        public async void CheckDevicesList()
        {
            List<string> listToRemove = await JJDeviceClass.GetUnavailableListEntries(_DevicesList);
            List<JJDeviceClass> listToAdd = await JJDeviceClass.GetAvailableListEntries(_DevicesList);

            foreach (string connId in listToRemove)
            {
                for (int i = 0; i < _DevicesList.Count; i++)
                {
                    if (_DevicesList[i].ConnId == connId)
                    {
                        _DevicesList.RemoveAt(i);
                        break;
                    }
                }
            }

            listToAdd.ForEach(deviceToAdd =>
            {
                _DevicesList.Add(deviceToAdd);
            });

            JJDeviceClass.CheckRestartAllProfileFile(ref _DevicesList);
        }

        public async void UpdateUpdaterList(bool initialization = false)
        {
            List<string> listToRemove = await JJManager.Class.App.Updater.GetUnavailableListEnties(_UpdaterList);
            List<JJManager.Class.App.Updater> listToAdd = await JJManager.Class.App.Updater.GetAvailableListEntries(_UpdaterList);

            listToRemove.ForEach(updaterToRemove =>
            {
                for (int i = 0; i < _UpdaterList.Count; i++)
                {
                    if (_UpdaterList[i].ConnId == updaterToRemove)
                    {
                        _UpdaterList.RemoveAt(i);
                        break;
                    }
                }
            });

            listToAdd.ForEach(updaterToAdd =>
            {
                _UpdaterList.Add(updaterToAdd);
            });

            if (initialization)
            {
                foreach (JJManager.Class.App.Updater updater in _UpdaterList)
                {
                    if (updater is JJManager.Class.App.SoftwareUpdater softwareUpdater && softwareUpdater.Name == Assembly.GetEntryAssembly().GetName().Name && softwareUpdater.NeedsUpdate)
                    {
                        Visible = false;
                        softwareUpdater.ShowNotificationForm(this);
                    }
                }
            }
        }

        public bool ChangeDeviceConnection(string connId, bool connectionStatus, bool updateListStatusOnly = false)
        {
            int selectedLvDevicesIndex = lvDevices.SelectedIndices[0];
            int selectedDeviceObjIndex = -1;
            bool successfullyChanged = false;

            for (int j = 0; j < _DevicesList.Count; j++)
            {
                if (_DevicesList[j].ConnId == connId)
                {
                    selectedDeviceObjIndex = j;

                    if (connectionStatus != _DevicesList[j].IsConnected && !updateListStatusOnly)
                    {
                        if (connectionStatus)
                        {
                            successfullyChanged = _DevicesList[j].Connect();
                        }
                        else
                        {
                            successfullyChanged = _DevicesList[j].Disconnect();
                        }
                    }
                }
            }

            if (successfullyChanged || updateListStatusOnly)
            {
                lvDevices.Items[selectedLvDevicesIndex].SubItems[3].Text = (_DevicesList[selectedDeviceObjIndex].IsConnected ? "Conectado" : "Desconectado");
                return true;
            }

            return false;
        }
        #endregion

        #region LoadAndManipulatingControls
        private void DisableAllForms()
        {
            foreach (Control control in Controls)
            {
                control.Enabled = false;
            }
        }

        public void EnableAllForms()
        {
            foreach (Control control in Controls)
            {
                if (control.Name == "btnConnChanger")
                {
                    control.Enabled = lvDevices.SelectedIndices.Count > 0 ? true : false;
                }
                else if (control.Name == "BtnUpdateDevice")
                {
                    control.Enabled = lvDevicesToUpdate.SelectedIndices.Count > 0 ? true : false;
                }
                else
                {
                    control.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Used to load style of dgvLog (List of log files)
        /// </summary>
        private void LoadLogData()
        {
            // Double buffer, to not flick buttons on hover
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, dgvLog, new object[] { true });

            dgvLog.EnableHeadersVisualStyles = false;

            dgvLog.BackgroundColor = MaterialSkinManager.Instance.BackgroundColor;

            dgvLog.RowsDefaultCellStyle.SelectionBackColor = materialSkinManager.Theme == MaterialSkinManager.Themes.DARK ? MaterialSkinManager.Instance.BackgroundColor.Lighten((float)0.2) : MaterialSkinManager.Instance.BackgroundColor.Darken((float)0.2);
            dgvLog.RowsDefaultCellStyle.SelectionForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;

            dgvLog.RowsDefaultCellStyle.BackColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvLog.RowsDefaultCellStyle.ForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;

            dgvLog.ColumnHeadersDefaultCellStyle.SelectionBackColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvLog.ColumnHeadersDefaultCellStyle.SelectionForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;


            dgvLog.ColumnHeadersDefaultCellStyle.BackColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvLog.ColumnHeadersDefaultCellStyle.ForeColor = MaterialSkinManager.Instance.BackgroundAlternativeColor;

            dgvLog.ColumnHeadersDefaultCellStyle.Font = new Font(dgvLog.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
            dgvLog.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 5, 0, 5);
            dgvLog.AdvancedColumnHeadersBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
            dgvLog.AdvancedColumnHeadersBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
            dgvLog.AdvancedColumnHeadersBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;

            dgvLog.CellClick += DgvLog_CellClick;
            dgvLog.CellMouseEnter += DgvLog_CellMouseEnter;
            dgvLog.CellMouseLeave += DgvLog_CellMouseLeave;
            dgvLog.CellPainting += DgvLog_CellPainting;
            dgvLog.MouseMove += DgvLog_MouseMove;

            RestartLogEntries();
        }

        /// <summary>
        /// Fill and format buttons on dvgLog (List of log files)
        /// </summary>
        /// <param name="e">DataGridView Cell Paint Event</param>
        /// <param name="iconUnicode">Unicode of FontAwesome icon</param>
        /// <param name="disableOnIndex">Disable the button in a especific index (most used in orders)</param>
        private void CreateStyledButtonOnDataViewGrid(DataGridViewCellPaintingEventArgs e, string iconUnicode, int disableOnIndex = -1)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);

            int margin = 1;
            int borderRadius = 4; // Adjust the border radius as needed
            Rectangle cellBounds = e.CellBounds;
            Rectangle buttonBounds = new Rectangle(
                cellBounds.X + margin,
                cellBounds.Y + margin,
                cellBounds.Width - 2 * margin,
                cellBounds.Height - 2 * margin
            );

            // Create a GraphicsPath for rounded rectangle
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(buttonBounds.X, buttonBounds.Y, borderRadius, borderRadius, 180, 90);
                path.AddArc(buttonBounds.X + buttonBounds.Width - borderRadius, buttonBounds.Y, borderRadius, borderRadius, 270, 90);
                path.AddArc(buttonBounds.X + buttonBounds.Width - borderRadius, buttonBounds.Y + buttonBounds.Height - borderRadius, borderRadius, borderRadius, 0, 90);
                path.AddArc(buttonBounds.X, buttonBounds.Y + buttonBounds.Height - borderRadius, borderRadius, borderRadius, 90, 90);
                path.CloseFigure();

                bool isHovered = buttonBounds.Contains(_mousePosition);
                bool isDisabled = disableOnIndex == e.RowIndex;

                // Fill the button background with color
                if (!isDisabled)
                {
                    e.Graphics.FillPath(isHovered ? materialSkinManager.ColorScheme.LightPrimaryBrush : materialSkinManager.ColorScheme.PrimaryBrush, path);
                }
                else
                {
                    e.Graphics.FillPath(new SolidBrush(materialSkinManager.BackgroundColor.Darken((float).2)), path);
                }

                TextRenderer.DrawText(
                    e.Graphics,
                    iconUnicode,
                    FontAwesome.UseSolid(8),
                    buttonBounds,
                    (isDisabled ? materialSkinManager.TextDisabledOrHintColor.Darken((float).5) : e.CellStyle.ForeColor),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );

                e.Graphics.DrawPath(materialSkinManager.ColorScheme.DarkPrimaryPen, path);
            }

            e.Handled = true; // Indicate that the painting is handled
        }

        /// <summary>
        /// Responsable to fill dvgLog (List of log files)
        /// </summary>
        private void RestartLogEntries()
        {
            dgvLog.Rows.Clear();

            IEnumerable<string[]> logs = Log.GetModulesInfo();

            foreach (string[] log in logs)
            {
                dgvLog.Rows.Add(log[0], log[1], "Abrir Log", "Remover Log");
            }

            btnRemoveAllLogs.Enabled = (logs.Count() > 0);
        }
        #endregion

        #region TickEvents
        private void FillListsTimer_tick(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (Visible)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            if (tabMain.SelectedTab.Name == "tabConnect")
                            {
                                CheckDevicesList();
                            }

                            if (tabMain.SelectedTab.Name == "tabUpdate")
                            {
                                UpdateUpdaterList();
                            }

                            if (tabMain.SelectedTab.Name == "tabOptions")
                            {
                                RestartLogEntries();
                            }
                        });
                    }
                    else
                    {
                        if (tabMain.SelectedTab.Name == "tabConnect")
                        {
                            CheckDevicesList();
                        }

                        if (tabMain.SelectedTab.Name == "tabUpdate")
                        {
                            UpdateUpdaterList();
                        }

                        if (tabMain.SelectedTab.Name == "tabOptions")
                        {
                            RestartLogEntries();
                        }
                    }
                }
            });
        }
        #endregion

        #region ButtonEvents
        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            notifyIcon.Hide();
            Visible = true;
            BringToFront();
        }

        private void SwtThemeColor_CheckedChanged(object sender, EventArgs e)
        {
            if (SwtThemeColor.Checked)
            {
                SwtThemeColor.Text = "Escuro";
                materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            }
            else
            {
                SwtThemeColor.Text = "Claro";
                materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            }

            ConfigClass.Theme.Update((SwtThemeColor.Checked ? "dark" : "light"));
        }

        private void BtnUpdateDevice_Click(object sender, EventArgs e)
        {
            string connId = lvDevicesToUpdate.SelectedItems[0].SubItems[0].Text;

            foreach (JJManager.Class.App.Updater updater in _UpdaterList)
            {
                if (connId == updater.ConnId)
                {
                    Thread thr = new Thread(() => {

                        string whatIsUpdating = "";

                        switch (updater.Type)
                        {
                            case Class.App.Updater.UpdaterType.Plugin:
                                whatIsUpdating = "atualização do plugin";
                                break;
                            case Class.App.Updater.UpdaterType.Device:
                                whatIsUpdating = "atualização do dispositivo";
                                break;
                            case Class.App.Updater.UpdaterType.Program:
                                whatIsUpdating = "atualização do programa";
                                break;
                        }

                        this.Invoke((MethodInvoker)delegate {
                            DisableAllForms();
                        });

                        DialogResult dialogResult = MessageBox.Show("Você deseja realizar a " + whatIsUpdating + " '" + updater.Name + "'?", whatIsUpdating.ToUpperInvariant(), MessageBoxButtons.YesNo);

                        if (dialogResult == DialogResult.Yes)
                        {
                            updater.Update(this);

                            this.Invoke((MethodInvoker)delegate {
                                txtStatusUpdate.Text = "Baixando " + whatIsUpdating + " '" + updater.Name + "...";
                            });
                        }
                        else
                        {
                            this.Invoke((MethodInvoker)delegate {
                                EnableAllForms();
                                txtStatusUpdate.Text = "";
                            });
                        }
                    });

                    thr.Name = "Updater_" + updater.ConnId;
                    thr.Start();

                    break;
                }
            }
        }

        private void btnConnChanger_Click(object sender, EventArgs e)
        {
            if (lvDevices.SelectedIndices.Count == 0)
            {
                return;
            }

            int lvDeviceSelected = lvDevices.SelectedIndices[0];

            string id = lvDevices.Items[lvDeviceSelected].SubItems[0].Text;
            bool actualConnectionStatus = (lvDevices.Items[lvDeviceSelected].SubItems[3].Text == "Conectado");

            btnConnChanger.Enabled = false;
            btnConnChanger.Text = (!actualConnectionStatus ? "Conectando..." : "Desconectando...");

            ChangeDeviceConnection(id, !actualConnectionStatus);

            actualConnectionStatus = (lvDevices.Items[lvDeviceSelected].SubItems[3].Text == "Conectado");

            btnConnChanger.Text = (!actualConnectionStatus ? "Conectar" : "Desconectar");
            btnConnChanger.Enabled = true;
        }

        private void btnEditDevice_Click(object sender, EventArgs e)
        {
            if (lvDevices.SelectedIndices.Count == 0)
            {
                return;
            }

            JJDeviceClass deviceSelected = null;
            string id = lvDevices.SelectedItems[0].SubItems[0].Text;

            switch (lvDevices.SelectedItems[0].SubItems[2].Text)
            {
                case "Bluetooth":
                    //deviceSelected = _BtDevicesList.First(device => device.ConnId == id);
                    break;
                case "HID":
                    deviceSelected = _DevicesList.First(device => device.ConnId == id);
                    break;
            }

            deviceSelected?.OpenEditWindow(this);
        }

        private void btnSearchBluetooth_Click(object sender, EventArgs e)
        {
            txtStatus.Text = "Buscando dispositivos bluetooth...";

            DialogResult dialogResult = MessageBox.Show("Ao realizar a busca por dispositivos JohnJohn 3D via bluetooth, o JJManager pode parar de responder por alguns segundos até concluir a busca, deseja continuar?", "Busca de dispositivos bluetooth", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {

                //CheckDevicesListBluetooth();
            }

            txtStatus.Text = "";
        }

        private void swtStartOnBoot_CheckedChanged(object sender, EventArgs e)
        {
            swtStartOnBoot.Text = swtStartOnBoot.Checked ? "Sim" : "Não";
            ConfigClass.StartOnBoot.Update(swtStartOnBoot.Checked);
        }

        private void btnRemoveAllLogs_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Você deseja realizar a limpeza de todos os logs? Lembre-se que esta ação é irreversível.", "Limpeza de logs", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                Log.CleanLogs();
                RestartLogEntries();
            }
        }

        private void DgvLog_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string moduleName = dgvLog.Rows[e.RowIndex].Cells[0].Value.ToString();

                if (e.ColumnIndex == dgvLog.Columns["dgvLogOpen"].Index) // Index of your button column
                {
                    Log.Open(moduleName);
                }

                if (e.ColumnIndex == dgvLog.Columns["dgvLogRemove"].Index) // Index of your button column
                {
                    DialogResult result = MessageBox.Show($"Você deseja excluir o log do módulo '{moduleName}'?", "Confirmação de exclusão", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        Log.CleanLog(moduleName);
                        RestartLogEntries();
                    }
                }
            }
        }
        #endregion

        #region OthersEvents
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && _DevicesList.Any(device => device.IsConnected))
            {
                e.Cancel = true;
                notifyIcon.Show();
                Visible = false;
            }
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            SwtThemeColor.Checked = ConfigClass.Theme.SelectedTheme == MaterialSkinManager.Themes.DARK ? true : false;
            swtStartOnBoot.Checked = ConfigClass.StartOnBoot.OnBoot;

            CheckDevicesList();
            UpdateUpdaterList(true);

            fillListsTimer.Start();
            EnableAllForms();
        }

        private void lvDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvDevices.SelectedIndices.Count == 0)
            {
                btnConnChanger.Enabled = false;
                btnEditDevice.Enabled = false;
                btnConnChanger.Text = "Conectar/Desconectar";
                return;
            }

            JJDeviceClass deviceSelected = null;
            string id = lvDevices.SelectedItems[0].SubItems[0].Text;

            switch (lvDevices.SelectedItems[0].SubItems[2].Text)
            {
                case "Bluetooth":
                    //deviceSelected = _BtDevicesList.First(device => device.ConnId == id);
                    break;
                case "HID":
                    deviceSelected = _DevicesList.First(device => device.ConnId == id);
                    break;
            }

            btnConnChanger.Text = deviceSelected.IsConnected ? "Desconectar" : "Conectar";
            btnConnChanger.Enabled = true;
            btnEditDevice.Enabled = true;
        }

        private void lvDevicesToUpdate_SelectedIndexChanged(object sender, EventArgs e)
        {
            LvDevicesToUpdateIndex = lvDevicesToUpdate.SelectedIndices.Count > 0 ? lvDevicesToUpdate.SelectedIndices[0] : -1;

            if (LvDevicesToUpdateIndex == -1 || lvDevicesToUpdate.SelectedItems[0].SubItems[3].Text == "N/A")
            {
                BtnUpdateDevice.Enabled = false;
                return;
            }

            BtnUpdateDevice.Enabled = true;
        }

        private void DgvLog_MouseMove(object sender, MouseEventArgs e)
        {
            _mousePosition = e.Location;

            var hitTest = dgvLog.HitTest(e.X, e.Y);
            if (hitTest.Type == DataGridViewHitTestType.Cell)
            {
                dgvLog.Invalidate(dgvLog.GetCellDisplayRectangle(hitTest.ColumnIndex, hitTest.RowIndex, true));
            }
        }

        private void DgvLog_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == dgvLog.Columns["dgvLogOpen"].Index && e.RowIndex >= 0)
            {
                CreateStyledButtonOnDataViewGrid(e, "\uf303");
            }

            if (e.ColumnIndex == dgvLog.Columns["dgvLogRemove"].Index && e.RowIndex >= 0)
            {
                CreateStyledButtonOnDataViewGrid(e, "\uf00d");
            }
        }

        private void DgvLog_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dgvLog.Rows[e.RowIndex].Selected = false;
            }
        }

        private void DgvLog_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dgvLog.ClearSelection();
                dgvLog.Rows[e.RowIndex].Selected = true;
            }
        }
        #endregion

        #region PublicFunctions
        /// <summary>
        /// Used to go to update tab when has a new update of JJManager
        /// </summary>
        public void GoToUpdateTab()
        {
            tabMain.SelectedTab = tabMain.TabPages[2];
        }
        #endregion
    }
}
