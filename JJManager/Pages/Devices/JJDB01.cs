using JJManager.Class;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;
using ProfileClass = JJManager.Class.App.Profile.Profile;
using JJDeviceClass = JJManager.Class.Devices.JJDevice;
using DashboardLedsOutput = JJManager.Class.App.Output.DashboardLeds.DashboardLeds;
using static System.Net.Mime.MediaTypeNames;
using JJManager.Class.App.Controls;
using JJManager.Properties;
using JJManager.Class.App.Controls.DrawImage;
using JJManager.Class.App.Fonts;
using JJManager.Class.App.Input;
using System.Collections.Specialized;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Reflection;
using Microsoft.SqlServer.Management.XEvent;
using JJManager.Class.App.Output.DashboardLeds;
using JJManager.Class.App.Output;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using JJManager.Class.App.Output.Leds;
using JJManager.Pages.App;
using static JJManager.Class.App.Output.Output;

namespace JJManager.Pages.Devices
{
    public partial class JJDB01 : MaterialForm
    {
        private JJDeviceClass _device = null;
        //private static AudioManager _audioManager = new AudioManager();
        private Thread thrTimers = null;
        private bool _IsInputSelected = false;
        private bool _IsCreateProfileOpened = false;
        private MaterialForm _parent = null;
        private Point _mousePosition = Point.Empty;
        private bool _isActionSelected = false;
        private BindingList<Output> _outputList = new BindingList<Output>();
        private JsonObject _data = null;
        private bool _saveData = false;
        private int _ledSelected = -1;
        private int _outputIdSelected = -1;
        private bool _isCreateProfileOpened = false;

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        private AppModulesTimer JoystickReceiver = null;
        private AppModulesNotifyIcon notifyIcon = null;
        #endregion

        public JJDB01 (MaterialForm parent, JJDeviceClass device)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            _device = device;
            _parent = parent;


            // Fill Forms
            LoadProfiles();

            //// Events
            //FormClosing += new FormClosingEventHandler(JJBP06_FormClosing);
            ////FormClosed += new FormClosedEventHandler(JJBP06_FormClosed);

            //ShowProfileConfigs();
            // Double buffer, to not flick buttons on hover
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, dgvActions, new object[] { true });

            dgvActions.AutoGenerateColumns = false;
            dgvActions.DataSource = _outputList;
            dgvActions.EnableHeadersVisualStyles = false;

            dgvActions.BackgroundColor = MaterialSkinManager.Instance.BackgroundColor;
            //dgvActions.ForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;
            //dgvActions.GridColor = MaterialSkinManager.Instance.BackgroundColor;

            dgvActions.RowsDefaultCellStyle.SelectionBackColor = materialSkinManager.Theme == MaterialSkinManager.Themes.DARK ? MaterialSkinManager.Instance.BackgroundColor.Lighten((float)0.2) : MaterialSkinManager.Instance.BackgroundColor.Darken((float)0.2);
            dgvActions.RowsDefaultCellStyle.SelectionForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;

            dgvActions.RowsDefaultCellStyle.BackColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvActions.RowsDefaultCellStyle.ForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;

            dgvActions.ColumnHeadersDefaultCellStyle.SelectionBackColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvActions.ColumnHeadersDefaultCellStyle.SelectionForeColor = MaterialSkinManager.Instance.TextMediumEmphasisColor;


            dgvActions.ColumnHeadersDefaultCellStyle.BackColor = MaterialSkinManager.Instance.BackgroundColor;
            dgvActions.ColumnHeadersDefaultCellStyle.ForeColor = MaterialSkinManager.Instance.BackgroundAlternativeColor;

            dgvActions.ColumnHeadersDefaultCellStyle.Font = new Font(dgvActions.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
            dgvActions.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 5, 0, 5);
            dgvActions.AdvancedColumnHeadersBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
            dgvActions.AdvancedColumnHeadersBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
            dgvActions.AdvancedColumnHeadersBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;

            dgvActions.CellClick += DgvActions_CellClick;
            dgvActions.CellMouseEnter += DgvActions_CellMouseEnter;
            dgvActions.CellMouseLeave += DgvActions_CellMouseLeave;
            dgvActions.CellPainting += DgvActions_CellPainting;
            dgvActions.MouseMove += DgvActions_MouseMove;
            SetEvents();
            LoadFormData();
        }

        public void SetEvents()
        {
            #region Profiles
            BtnAddProfile.Click += BtnAddProfile_Click;
            BtnRemoveProfile.Click += BtnRemoveProfile_Click;
            CmbBoxSelectProfile.DropDown += CmbBoxSelectProfile_DropDown;
            CmbBoxSelectProfile.SelectedIndexChanged += CmbBoxSelectProfile_SelectedIndexChanged;
            #endregion

            #region GeneralForms
            SldLedBrightness.onValueChanged += SldLedBrightness_onValueChanged;

            BtnAddLedAction.Click += BtnAddLedAction_Click;

            foreach (Control control in PnlJJDB01.Controls)
            {
                if (control is DrawImage && control.Name.StartsWith("ImgJJDB01Input"))
                {
                    ((DrawImage)control).Click += ImgJJDB01Input_Click;
                    ((DrawImage)control).MouseEnter += ImgJJDB01Input_MouseEnter; ;
                    ((DrawImage)control).MouseLeave += ImgJJDB01Input_MouseLeave; ;
                }
            }
            #endregion

            #region Window
            FormClosing += JJBSlim_A_FormClosing;
            Resize += JJBSlim_A_Resize;
            BtnClose.Click += BtnClose_Click;
            //FormClosed += new FormClosedEventHandler(JJBP06_FormClosed);
            #endregion
        }

        public void FillForm()
        {
            #region Profile
            foreach (string Profile in ProfileClass.GetProfilesList(_device.ProductId))
            {
                CmbBoxSelectProfile.Items.Add(Profile);
            }

            if (CmbBoxSelectProfile.Items.Count == 0)
            {
                CmbBoxSelectProfile.Items.Add(new ProfileClass(_device, "Perfil Padrão", true));
                CmbBoxSelectProfile.SelectedIndex = 0;
            }
            else
            {
                CmbBoxSelectProfile.SelectedIndex = CmbBoxSelectProfile.FindStringExact(_device.Profile.Name); ;
            }

            UpdateForm();
            #endregion

            #region General
            // Nothing to do...
            #endregion

        }

        public void UpdateForm()
        {
            _saveData = false;

            #region Individual Buttons
            if (_ledSelected > -1)
            {
                var outputsList = _device.Profile.Outputs.ToList();

                Output output = outputsList.FirstOrDefault(x => x.Led?.LedsGrouped.Contains(_ledSelected) == true) ?? null;
            }
            #endregion

            #region General
            if (_device.Profile.Data.ContainsKey("brightness"))
            {
                switch (_device.Profile.Data["brightness"].GetValueKind())
                {
                    case JsonValueKind.String:
                        string brightnessString = _device.Profile.Data["brightness"].GetValue<string>();

                        //if (int.TryParse(brightnessString, out int brightnessValue) && (brightnessValue < cmdBoxLedMode.Items.Count))
                        //{
                        //    SldLedBrightness.Value = brightnessValue;
                        //}

                        break;
                    case JsonValueKind.Number:
                        SldLedBrightness.Value = _device.Profile.Data["brightness"].GetValue<int>();
                        break;
                }
            }
            #endregion

            _saveData = true;
        }

        #region Event Functions (General)
        private void BtnAddLedAction_Click(object sender, EventArgs e)
        {
            LedRgbAction ledAction = new LedRgbAction(this, null, _ledSelected);
            DialogResult result = ledAction.ShowDialog();

            if (result == DialogResult.OK)
            {
                Output output = _device.Profile.Outputs.FirstOrDefault(x => x.Mode == Output.OutputMode.None);

                if (output == null)
                {
                    App.MessageBox.Show(this, "Problema Na Busca De Dados", "Ocorreu um problema na busca dos dados referentes a está saída, tente novamente realizar o processo.\n\nCaso o erro persista contate-nos.");
                }
                else
                {
                    // Used to get last order position
                    ledAction.LedToSave.Order = dgvActions.RowCount;

                    int index = _device.Profile.Outputs.IndexOf(output);
                    _device.Profile.Outputs[index].SetToLeds(output.Id, Leds.LedType.RGB);
                    _device.Profile.Outputs[index].Led.JsonToLeds(ledAction.LedToSave.objectToJson());
                    _device.Profile.Outputs[index].Changed = true;
                    _device.Profile.Outputs[index].Save();
                    LoadFormData();
                }
            }
        }

        private void SldLedBrightness_onValueChanged(object sender, int newValue)
        {
            JsonObject profileData = _device.Profile.Data;
            int brightness = ((MaterialSlider)sender).Value;

            if (!profileData.ContainsKey("brightness"))
            {
                profileData.Add("brightness", brightness);
            }
            else
            {
                profileData["brightness"] = brightness;
            }

            _device.Profile.Update(profileData);
        }
        #endregion

        #region Event Functions (Individual Buttons)
        
        #endregion

        #region Event Functions (Window)
        private void JJBSlim_A_Resize(object sender, EventArgs e)
        {
            PnlJJDB01.Size = new Size(this.ClientSize.Width / 2 - 20, this.ClientSize.Height - 122 - 60);
            PnlJJDB01.Location = new Point(10, 120);

            PnlConfigs.Size = new Size(this.ClientSize.Width / 2 - 20, this.ClientSize.Height - 122 - 60);
            PnlConfigs.Location = new Point(this.ClientSize.Width / 2 + 10, 120);
        }

        private void JJBSlim_A_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parent.Visible = true;
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region Event Functions (Profile)
        private void CmbBoxSelectProfile_DropDown(object sender, EventArgs e)
        {
            int selectedIndex = CmbBoxSelectProfile.SelectedIndex;

            CmbBoxSelectProfile.Items.Clear();

            foreach (String Profile in ProfileClass.GetProfilesList(_device.ProductId))
                CmbBoxSelectProfile.Items.Add(Profile);

            CmbBoxSelectProfile.SelectedIndex = selectedIndex;
        }

        private void CmbBoxSelectProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CmbBoxSelectProfile.SelectedIndex != -1)
            {
                _device.Profile = new ProfileClass(_device, CmbBoxSelectProfile.SelectedItem.ToString(), true);
                UpdateForm();
            }

        }

        private void BtnAddProfile_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(() => {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        if (!_isCreateProfileOpened)
                        {
                            _isCreateProfileOpened = true;
                            CreateProfile createProfile = new CreateProfile(_device);
                            DialogResult result = createProfile.ShowDialog();

                            if (result == DialogResult.OK)
                            {
                                FillForm();
                            }

                            _isCreateProfileOpened = false;
                        }
                    });
                }
                else
                {
                    if (!_isCreateProfileOpened)
                    {
                        _isCreateProfileOpened = true;
                        CreateProfile createProfile = new CreateProfile(_device);
                        DialogResult result = createProfile.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            FillForm();
                        }

                        _isCreateProfileOpened = false;
                    }
                }
                Thread.CurrentThread.Abort();
            });
            thr.Name = "Add_Profile";
            thr.Start();
        }

        private void BtnRemoveProfile_Click(object sender, EventArgs e)
        {
            if (CmbBoxSelectProfile.SelectedIndex == -1)
            {
                Pages.App.MessageBox.Show(this, "Selecione um Perfil", "Selecione um perfil para excluí-lo.");
                return;
            }

            if (CmbBoxSelectProfile.Items.Count == 1)
            {
                Pages.App.MessageBox.Show(this, "Não Pode Excluir", "Você possui apenas um perfil e este não pode ser excluído.");
                return;
            }

            DialogResult dialogResult = Pages.App.MessageBox.Show(this, "Exclusão de Perfil", "Você está prestes a excluir o Perfil '" + CmbBoxSelectProfile.SelectedItem.ToString() + "', deseja continuar?", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                string profileNameToExclude = CmbBoxSelectProfile.SelectedItem.ToString();

                CmbBoxSelectProfile.Items.Remove(CmbBoxSelectProfile.SelectedItem);

                string profileNameToActive = CmbBoxSelectProfile.Items[(CmbBoxSelectProfile.Items[0] == CmbBoxSelectProfile.SelectedItem ? 1 : 0)].ToString();

                _device.Profile = new ProfileClass(_device, profileNameToActive, true);

                ProfileClass.Delete(profileNameToExclude, _device.ProductId);

                CmbBoxSelectProfile.SelectedIndex = 0;

                Pages.App.MessageBox.Show(this, "Perfil Excluído", "Perfil excluído com sucesso!");

                FillForm();
            }
        }
        #endregion

        #region Event Functions (Img Buttons)
        private void ImgJJDB01Input_MouseLeave(object sender, EventArgs e)
        {
            if (int.TryParse(((DrawImage)sender).Tag?.ToString(), out int index) && _ledSelected != index)
            {
                ((DrawImage)sender).Image = Resources.JJDB01_led;
            }
        }

        private void ImgJJDB01Input_MouseEnter(object sender, EventArgs e)
        {
            ((DrawImage)sender).Image = Resources.JJDB01_led_hover;
        }

        private void ImgJJDB01Input_Click(object sender, EventArgs e)
        {
            _saveData = false;
            foreach (Control control in PnlJJDB01.Controls)
            {
                if (control is DrawImage picture && control.Name.StartsWith("ImgJJDB01Input") && int.TryParse(((DrawImage)picture).Tag?.ToString(), out int pictureIndex) && pictureIndex == _ledSelected)
                {
                    picture.Image = Resources.JJDB01_led;
                }
            }

            if (int.TryParse(((DrawImage)sender).Tag?.ToString(), out int led) && _ledSelected != led && led > -1)
            {
                //for (int i = 0; i < _device.Profile.Outputs.Count; i++)
                //{
                //    if (_device.Profile.Outputs[i]?.Led?.LedsGrouped.Contains(led) == true)
                //    {
                //        CmbSimHubProps.SelectedValue = _device.Profile.Outputs[i]?.Led?.Property ?? string.Empty;
                //        break;
                //    }
                //}

                _ledSelected = led;
                LoadFormData();
                ((DrawImage)sender).Image = Resources.JJDB01_led_hover;

            }
            else
            {
                _ledSelected = -1;
            }
            _saveData = true;
        }
        #endregion

        private void DgvActions_MouseMove(object sender, MouseEventArgs e)
        {
            _mousePosition = e.Location;

            var hitTest = dgvActions.HitTest(e.X, e.Y);
            if (hitTest.Type == DataGridViewHitTestType.Cell)
            {
                dgvActions.Invalidate(dgvActions.GetCellDisplayRectangle(hitTest.ColumnIndex, hitTest.RowIndex, true));
            }
        }

        private void CreateStyledButtonOnDataViewGrid(DataGridViewCellPaintingEventArgs e, string iconUnicode, int disableOnIndex = -1)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);

            // Draw the icon as text in the center of the button
            //e.Graphics.FillRectangle(materialSkinManager.ColorScheme.PrimaryBrush, e.CellBounds); // Change 'Green' to any desired color

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


        private void DgvActions_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == dgvActions.Columns["dgvActionEdit"].Index && e.RowIndex >= 0)
            {
                CreateStyledButtonOnDataViewGrid(e, "\uf303");
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionRemove"].Index && e.RowIndex >= 0)
            {
                CreateStyledButtonOnDataViewGrid(e, "\uf00d");
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionMoveUp"].Index && e.RowIndex >= 0)
            {
                CreateStyledButtonOnDataViewGrid(e, "\uf077", 0);
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionMoveDown"].Index && e.RowIndex >= 0)
            {
                CreateStyledButtonOnDataViewGrid(e, "\uf078", (dgvActions.RowCount - 1));
            }
        }

        private void DgvActions_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dgvActions.Rows[e.RowIndex].Selected = false;
            }
        }

        private void DgvActions_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dgvActions.ClearSelection();
                dgvActions.Rows[e.RowIndex].Selected = true;
            }
        }

        private void DgvActions_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvActions.Columns["dgvActionEdit"].Index) // Index of your button column
            {
                OpenActionModal(int.Parse(dgvActions.Rows[e.RowIndex].Cells["Id"].Value.ToString()));
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionRemove"].Index) // Index of your button column
            {
                DialogResult result = Pages.App.MessageBox.Show(this, "Confirmação de Exclusão", "Você deseja excluir está ação?", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    _device.Profile.ExcludeOutput(int.Parse(dgvActions.Rows[e.RowIndex].Cells["Id"].Value.ToString()), Output.OutputMode.Leds);
                    LoadFormData();
                }
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionMoveUp"].Index) // Index of your button column
            {
                if (e.RowIndex == 0)
                {
                    return;
                }

                _device.Profile.MoveOutput(uint.Parse(dgvActions.Rows[e.RowIndex].Cells["Id"].Value.ToString()), (e.RowIndex - 1), dgvActions.RowCount - 1, Output.OutputMode.Leds);

                LoadFormData();
                dgvActions.Rows[(e.RowIndex - 1)].Selected = true;
            }

            if (e.ColumnIndex == dgvActions.Columns["dgvActionMoveDown"].Index) // Index of your button column
            {
                if (e.RowIndex == (dgvActions.RowCount - 1))
                {
                    return;
                }

                _device.Profile.MoveOutput(uint.Parse(dgvActions.Rows[e.RowIndex].Cells["Id"].Value.ToString()), (e.RowIndex + 1), dgvActions.RowCount - 1, Output.OutputMode.Leds);

                LoadFormData();
                dgvActions.Rows[(e.RowIndex + 1)].Selected = true;
            }
        }

        private void LoadProfiles()
        {
            foreach (String Profile in ProfileClass.GetProfilesList(_device.ProductId))
                CmbBoxSelectProfile.Items.Add(Profile);

            if (CmbBoxSelectProfile.Items.Count == 0)
            {
                CmbBoxSelectProfile.Items.Add(new ProfileClass(_device, "Perfil Padrão", true));
                CmbBoxSelectProfile.SelectedIndex = 0;
            }
            else
            {
                CmbBoxSelectProfile.SelectedIndex = CmbBoxSelectProfile.FindStringExact(_device.Profile.Name); ;
            }

        }

        private void LoadFormData()
        {
            _outputList.Clear();
            //_device.Profile.OrderOutputsBy(Output.OutputMode.Leds);

            // Ensure BindingList is initialized with Outputs from Profile
            foreach (Output item in _device.Profile?.Outputs
                .Where(o => o.Mode == Output.OutputMode.Leds && o.Led.LedsGrouped.Contains(_ledSelected))
                .ToList().OrderBy(x => x.Led.Order))
            {
                _outputList.Add(item);
            }

            SldLedBrightness.Value = _device.Profile.Data.AsObject().ContainsKey("led_brightness")
                ? _device.Profile.Data.AsObject()["led_brightness"].GetValue<int>()
                : 50;

            //Save();

            dgvActions.Refresh();
        }

        public void OpenActionModal(int idAction)
        {
            Output output = _device.Profile?.Outputs?.Where(x => x.Id == idAction).FirstOrDefault();
            int index = _device.Profile.Outputs.IndexOf(output);

            if (output.Mode == OutputMode.None)
            {
                uint idSearched = _device.Profile?.Outputs?.First(x => x.Mode == OutputMode.None).Id ?? uint.MaxValue;

                if (idSearched == uint.MaxValue)
                {
                    Log.Insert("JJDB01", "Ocorreu um problema ao buscar um ID para o output especificado.");
                    throw new ArgumentNullException("Ocorreu um problema ao buscar um ID para o output especificado.");
                }

                output.SetToLeds(idSearched, Leds.LedType.RGB);
            }

            Pages.App.LedRgbAction dashboardLedsPage = new Pages.App.LedRgbAction(this, output.Led, _ledSelected);
            Visible = false;

            if (dashboardLedsPage.ShowDialog() == DialogResult.OK)
            {
                _device.Profile.Outputs[index].Name = dashboardLedsPage.LedToSave.PropertyName ?? "Nenhuma função selecionada";
                _device.Profile.Outputs[index].Led.Update(dashboardLedsPage.LedToSave.objectToJson());
                _device.Profile.Outputs[index].Changed = true;
                _device.Profile.Outputs[index].Save();
                _device.Profile.NeedsUpdate = true;
                LoadFormData();
            }

            _isActionSelected = false;
        }
    }
}
