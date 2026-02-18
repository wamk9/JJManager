using JJManager.Class.App.Controls.DrawImage;
using JJManager.Class.App.Output.Leds;
using JJManager.Class.App.Output;
using JJManager.Class;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;
using ProfileClass = JJManager.Class.App.Profile.Profile;
using JJDeviceClass = JJManager.Class.Devices.JJDevice;
using JJManager.Properties;
using JJManager.Class.App.Fonts;
using System.Drawing.Drawing2D;
namespace JJManager.Pages.App
{
    public partial class LedMonoAction : MaterialForm
    {
        private JJDeviceClass _device = null;
        private bool _isCreateProfileOpened = false;
        private MaterialForm _parent = null;
        private JsonObject _data = null;
        private bool _saveData = false;
        private int _ledSelected = -1;
        private MaterialSkinManager _materialSkinManager = null;
        public Leds LedToSave = null;

        public LedMonoAction(MaterialForm parent, Leds ledToEdit, int ledSelected)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            // MaterialDesign
            _materialSkinManager = MaterialSkinManager.Instance;
            _materialSkinManager.AddFormToManage(this);
            _materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            _materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            LedToSave = ledToEdit;
            _ledSelected = ledSelected;

            _parent = parent;
            Text = $"{(ledToEdit != null ? "Editando" : "Criando")} Ação Do Led";

            TxtAlertMessageIcon.Text = "\uf071";
            TxtAlertMessageIcon.Font = FontAwesome.UseSolid(18);
            TxtAlertDynamicBrightnessIcon.Text = "\uf05a";
            TxtAlertDynamicBrightnessIcon.Font = FontAwesome.UseSolid(18);

            PnlAlertMessage.BackColor = _materialSkinManager.CardsColor;
            TxtAlertMessageIcon.BackColor = _materialSkinManager.CardsColor;
            PnlAlertDynamicBrightness.BackColor = _materialSkinManager.CardsColor;
            TxtAlertDynamicBrightnessIcon.BackColor = _materialSkinManager.CardsColor;

            FillForm();
            SetEvents();
        }

        public void SetEvents()
        {
            #region Profiles
            //BtnAddProfile.Click += BtnAddProfile_Click;
            //BtnRemoveProfile.Click += BtnRemoveProfile_Click;
            //CmbBoxSelectProfile.DropDown += CmbBoxSelectProfile_DropDown;
            //CmbBoxSelectProfile.SelectedIndexChanged += CmbBoxSelectProfile_SelectedIndexChanged;
            #endregion

            #region GeneralForms
            CmbLedMode.SelectedIndexChanged += CmdLedMode_SelectedIndexChanged;
            CmbSimHubProps.SelectedIndexChanged += CmbSimHubProps_SelectedIndexChanged;
            //CmbModeIfSimHubEnabled.SelectedIndexChanged += CmbModeIfSimHubEnabled_SelectedIndexChanged;
            //SldLedBrightness.onValueChanged += SldLedBrightness_onValueChanged;

            //foreach (Control control in PnlJJBSlimA.Controls)
            //{
            //    if (control is DrawImage && control.Name.StartsWith("ImgJJSlimAInput"))
            //    {
            //        ((DrawImage)control).Click += ImgJJSlimAInput_Click;
            //        ((DrawImage)control).MouseEnter += ImgJJSlimAInput_MouseEnter; ;
            //        ((DrawImage)control).MouseLeave += ImgJJSlimAInput_MouseLeave; ;
            //    }
            //}
            #endregion

            #region Window
            FormClosing += JJBSlim_A_FormClosing;
            Resize += JJBSlim_A_Resize;
            BtnSave.Click += BtnSave_Click;
            BtnCancel.Click += BtnCancel_Click;

            //FormClosed += new FormClosedEventHandler(JJBP06_FormClosed);
            #endregion
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public void FillForm()
        {
            #region Profile
            //foreach (string Profile in ProfileClass.GetProfilesList(_device.ProductId))
            //{
            //    CmbBoxSelectProfile.Items.Add(Profile);
            //}

            //if (CmbBoxSelectProfile.Items.Count == 0)
            //{
            //    CmbBoxSelectProfile.Items.Add(new ProfileClass(_device, "Perfil Padrão", true));
            //    CmbBoxSelectProfile.SelectedIndex = 0;
            //}
            //else
            //{
            //    CmbBoxSelectProfile.SelectedIndex = CmbBoxSelectProfile.FindStringExact(_device.Profile.Name); ;
            //}

            #endregion

            #region General
            // Nothing to do...
            #endregion

            #region Individual Buttons
            //CmbModeIfSimHubEnabled.Enabled = false;
            //CmbSimHubProps.Enabled = false;
            //cmdBoxLedMode.Enabled = false;

            Dictionary<string, string> items = new Dictionary<string, string>();

            DatabaseConnection databaseConnection = new DatabaseConnection();

            JsonArray jsonResult = databaseConnection.RunSQLWithResults("SELECT name, jj_prop FROM dbo.simhub_properties");

            foreach (JsonObject obj in jsonResult)
            {
                items.Add(obj["name"].GetValue<string>(), obj["jj_prop"].GetValue<string>());
            }

            CmbSimHubProps.DataSource = new BindingSource(items, null);
            CmbSimHubProps.DisplayMember = "Key";
            CmbSimHubProps.ValueMember = "Value";

            //CmbModeIfSimHubEnabled.SelectedIndex = -1;
            //CmbSimHubProps.SelectedIndex = -1;
            //cmdBoxLedMode.SelectedIndex = -1;

            //CmbModeIfSimHubEnabled.Refresh();
            CmbSimHubProps.Refresh();

            // Configure LED Mode ComboBox with DataSource BEFORE UpdateForm()
            var ledModes = new[]
            {
                new { Text = "Desligado", Value = 0 },
                new { Text = "Sempre Ligado", Value = 1 },
                new { Text = "Pulsando", Value = 2 },
                new { Text = "Piscando", Value = 3 },
                new { Text = "Brilho Dinâmico", Value = 4 }
            };

            CmbLedMode.DataSource = ledModes;
            CmbLedMode.DisplayMember = "Text";
            CmbLedMode.ValueMember = "Value";
            CmbLedMode.Refresh();

            // Now call UpdateForm() to load values from LedToSave
            UpdateForm();

            //cmdBoxLedMode.Refresh();
            #endregion
        }

        public void UpdateForm()
        {
            _saveData = false;

            CmbSimHubProps.SelectedValue = LedToSave?.Property ?? "";
            CmbSimHubProps.Refresh();

            // Load LED Mode from LedToSave
            if (LedToSave != null)
            {
                CmbLedMode.SelectedValue = LedToSave.ModeIfEnabled;
                CmbLedMode.Refresh();
            }

            #region General
            //if (_device.Profile.Data.ContainsKey("brightness"))
            //{
            //    switch (_device.Profile.Data["brightness"].GetValueKind())
            //    {
            //        case JsonValueKind.String:
            //            string brightnessString = _device.Profile.Data["brightness"].GetValue<string>();

            //            if (int.TryParse(brightnessString, out int brightnessValue) && (brightnessValue < cmdBoxLedMode.Items.Count))
            //            {
            //                SldLedBrightness.Value = brightnessValue;
            //            }

            //            break;
            //        case JsonValueKind.Number:
            //            SldLedBrightness.Value = _device.Profile.Data["brightness"].GetValue<int>();
            //            break;
            //    }
            //}
            #endregion

            _saveData = true;
        }

        #region Event Functions (General)
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
        private void CmbModeIfSimHubEnabled_SelectedIndexChanged(object sender, EventArgs e)
        {
            Save();
        }

        private void CmbSimHubProps_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Save();
        }

        private void CmdLedMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            PnlAlertDynamicBrightness.Visible = ((int)(CmbLedMode?.SelectedValue ?? -1) == 4);

            //if (cmdBoxLedMode.SelectedIndex == 5)
            //{
            //    CmbModeIfSimHubEnabled.Enabled = true;
            //    CmbSimHubProps.Enabled = true;
            //}
            //else
            //{
            //    CmbModeIfSimHubEnabled.SelectedIndex = -1;
            //    CmbSimHubProps.SelectedIndex = -1;
            //    CmbModeIfSimHubEnabled.Refresh();
            //    CmbSimHubProps.Refresh();
            //}

            //Save();
        }
        #endregion

        #region Event Functions (Window)
        private void JJBSlim_A_Resize(object sender, EventArgs e)
        {
            //PnlJJBSlimA.Size = new Size(this.ClientSize.Width / 2 - 20, this.ClientSize.Height - 122 - 60);
            //PnlJJBSlimA.Location = new Point(10, 120);

            //PnlConfigs.Size = new Size(this.ClientSize.Width / 2 - 20, this.ClientSize.Height - 122 - 60);
            //PnlConfigs.Location = new Point(this.ClientSize.Width / 2 + 10, 120);
        }

        private void JJBSlim_A_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parent.Visible = true;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Save();
        }
        #endregion

        #region Event Functions (Profile)
        private void CmbBoxSelectProfile_DropDown(object sender, EventArgs e)
        {
            //int selectedIndex = CmbBoxSelectProfile.SelectedIndex;

            //CmbBoxSelectProfile.Items.Clear();

            //foreach (String Profile in ProfileClass.GetProfilesList(_device.ProductId))
            //    CmbBoxSelectProfile.Items.Add(Profile);

            //CmbBoxSelectProfile.SelectedIndex = selectedIndex;
        }

        private void CmbBoxSelectProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (CmbBoxSelectProfile.SelectedIndex != -1)
            //{
            //    _device.Profile = new ProfileClass(_device, CmbBoxSelectProfile.SelectedItem.ToString(), true);
            //    UpdateForm();
            //}

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
            //if (CmbBoxSelectProfile.SelectedIndex == -1)
            //{
            //    MessageBox.Show("Selecione um perfil para exclui-lo.");
            //    return;
            //}

            //if (CmbBoxSelectProfile.Items.Count == 1)
            //{
            //    MessageBox.Show("Você possui apenas um perfil e este não pode ser excluído.");
            //    return;
            //}

            //DialogResult dialogResult = MessageBox.Show("Você está prestes a excluir o Perfil '" + CmbBoxSelectProfile.SelectedItem.ToString() + "', deseja continuar?", "Exclusão de Perfil", MessageBoxButtons.YesNo);

            //if (dialogResult == DialogResult.Yes)
            //{
            //    string profileNameToExclude = CmbBoxSelectProfile.SelectedItem.ToString();

            //    CmbBoxSelectProfile.Items.Remove(CmbBoxSelectProfile.SelectedItem);

            //    string profileNameToActive = CmbBoxSelectProfile.Items[(CmbBoxSelectProfile.Items[0] == CmbBoxSelectProfile.SelectedItem ? 1 : 0)].ToString();

            //    _device.Profile = new ProfileClass(_device, profileNameToActive, true);

            //    ProfileClass.Delete(profileNameToExclude, _device.ProductId);

            //    CmbBoxSelectProfile.SelectedIndex = 0;

            //    MessageBox.Show("Perfil excluído com sucesso!");

            //    FillForm();
            //}
        }
        #endregion

        #region Event Functions (Img Buttons)
        private void ImgJJSlimAInput_MouseLeave(object sender, EventArgs e)
        {
            if (int.TryParse(((DrawImage)sender).Tag?.ToString(), out int index) && _ledSelected != index)
            {
                ((DrawImage)sender).Image = null;
            }
        }

        private void ImgJJSlimAInput_MouseEnter(object sender, EventArgs e)
        {
            ((DrawImage)sender).Image = Resources.JJB_Slim_A_hover;
        }

        private void ImgJJSlimAInput_Click(object sender, EventArgs e)
        {
            _saveData = false;
            //foreach (Control control in PnlJJBSlimA.Controls)
            //{
            //    if (control is DrawImage picture && control.Name.StartsWith("ImgJJSlimAInput") && int.TryParse(((DrawImage)picture).Tag?.ToString(), out int pictureIndex) && pictureIndex == _ledSelected)
            //    {
            //        picture.Image = null;
            //    }
            //}

            //CmbModeIfSimHubEnabled.SelectedIndex = -1;
            //CmbSimHubProps.SelectedIndex = -1;
            //cmdBoxLedMode.SelectedIndex = -1;

            //if (int.TryParse(((DrawImage)sender).Tag?.ToString(), out int led) && _ledSelected != led && led > -1)
            //{
            //    for (int i = 0; i < _device.Profile.Outputs.Count; i++)
            //    {
            //        if (_device.Profile.Outputs[i]?.Led?.LedsGrouped.Contains(led) == true)
            //        {
            //            cmdBoxLedMode.SelectedIndex = _device.Profile.Outputs[i]?.Led?.Mode ?? -1;
            //            CmbSimHubProps.SelectedValue = _device.Profile.Outputs[i]?.Led?.Property ?? string.Empty;
            //            CmbModeIfSimHubEnabled.SelectedIndex = _device.Profile.Outputs[i]?.Led?.ModeIfEnabled ?? -1;
            //            break;
            //        }
            //    }
            //    CmbModeIfSimHubEnabled.Enabled = (cmdBoxLedMode.SelectedIndex == 5);
            //    CmbSimHubProps.Enabled = (cmdBoxLedMode.SelectedIndex == 5);
            //    cmdBoxLedMode.Enabled = true;

            //    _ledSelected = led;
            //    ((DrawImage)sender).Image = Resources.JJB_Slim_A_hover;

            //}
            //else
            //{
            //    cmdBoxLedMode.Enabled = false;
            //    _ledSelected = -1;
            //}

            //cmdBoxLedMode.Refresh();
            //CmbModeIfSimHubEnabled.Refresh();
            //CmbSimHubProps.Refresh();

            _saveData = true;
        }
        #endregion

        private void Save()
        {
            if (_ledSelected < 0)
            {
                return;
            }

            JsonArray ledsGrouped = new JsonArray { _ledSelected };
            int mode = -1;
            int modeIfEnabled = (int)(CmbLedMode.SelectedValue ?? -1);
            string prop = CmbSimHubProps.SelectedValue as string ?? null;
            string propName = CmbSimHubProps.SelectedItem is KeyValuePair<string, string> selectedPair ? selectedPair.Key : null;
            int order = LedToSave?.Order ?? -1;
            string color = $"#ffffff";

            _data = new JsonObject
            {
                {
                    "leds", ledsGrouped
                },
                {
                    "color", color
                },
                {
                    "jj_prop", prop
                },
                {
                    "prop_name", propName
                },
                {
                    "order", order
                },
                {
                    "led_mode", mode
                },
                {
                    "led_type", "pwm"
                },
                {
                    "led_mode_simhub", modeIfEnabled
                }
            };

            if (LedToSave == null)
            {
                LedToSave = new Leds(_data);
            }
            else
            {
                LedToSave.JsonToLeds(_data);
            }

            DialogResult = DialogResult.OK;

            //if (_device.Profile.Outputs[_ledSelected].Mode != Output.OutputMode.Leds)
            //{
            //    _device.Profile.Outputs[_ledSelected].SetToLeds((uint) _ledSelected, Leds.LedType.PWM);
            //}

            //_device.Profile.Outputs[_ledSelected].Led.JsonToLeds(_data);
            //_device.Profile.Outputs[_ledSelected].Changed = true;
            //_device.Profile.Outputs[_ledSelected].Save();
        }
    }
}
