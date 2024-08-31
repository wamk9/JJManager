using JJManager.Class;
using JJManager.Class.App.Input.MacroKey.Keyboard;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;
using ProfileClass = JJManager.Class.App.Profile.Profile;

namespace JJManager.Pages.App
{
    public partial class MacroKeyAction : MaterialForm
    {
        private static ProfileClass _activeProfile;
        //private static AudioManager _audioManager = new AudioManager();
        private static DatabaseConnection _DatabaseConnection = new DatabaseConnection();
        private Thread thrTimers = null;
        private bool _IsInputSelected = false;
        private bool _IsCreateProfileOpened = false;
        private MaterialForm _parent = null;
        private uint _idInput = 0;
        private bool _keyboardRecEnabled = false;
        private Class.App.Input.MacroKey.MacroKeyAction _action = null;
        private ushort keyboardKeyCode = ushort.MaxValue;
        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        private AppModulesTimer JoystickReceiver = null;
        private AppModulesNotifyIcon notifyIcon = null;
        #endregion

        public Class.App.Input.MacroKey.MacroKeyAction Action
        {
            get => _action;
        }

        public MacroKeyAction(MaterialForm parent, Class.App.Input.MacroKey.MacroKeyAction action)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();
            Text = "Ação";

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            _parent = parent;
            _action = action;

            // Events
            FormClosing += new FormClosingEventHandler(MacroKey_FormClosing);
            //FormClosed += new FormClosedEventHandler(JJB01_V2_FormClosed);

            SetInputInfo();

            ChangeMouseInputs(tglMouse.Checked);
            ChangeKeyboardKeyInputs(tglKeyboardKey.Checked);
            ChangeKeyboardTextInputs(tglKeyboardText.Checked);
            ChangeWaitTimeInputs(tglWaitTime.Checked);
        }

        private void SetInputInfo()
        {
            if (_action != null)
            {
                switch (_action.Type)
                {
                    case Class.App.Input.MacroKey.MacroKeyAction.ActionType.Mouse:
                        tglMouse.Checked = true;
                        tbcAction.SelectedTab = tabMouse;
                        break;
                    case Class.App.Input.MacroKey.MacroKeyAction.ActionType.KeyboardKey:
                        tglKeyboardKey.Checked = true;
                        tbcAction.SelectedTab = tabKeyboardKey;
                        break;
                    case Class.App.Input.MacroKey.MacroKeyAction.ActionType.KeyboardText:
                        tbcAction.SelectedTab = tabKeyboardText;
                        tglKeyboardText.Checked = true;
                        break;
                    case Class.App.Input.MacroKey.MacroKeyAction.ActionType.Wait:
                        tbcAction.SelectedTab = TabWaitTime;
                        tglWaitTime.Checked = true;
                        break;
                }

                if (_action.Data != null)
                {
                    // Mouse
                    txbMouseX.Text = (_action.Data.ContainsKey("mouseX") ? _action.Data["mouseX"].GetValue<int>() : 0).ToString();
                    txbMouseY.Text = (_action.Data.ContainsKey("mouseY") ? _action.Data["mouseY"].GetValue<int>() : 0).ToString();
                    
                    if (_action.Data.ContainsKey("mouseClick"))
                    {
                        switch (_action.Data["mouseClick"].GetValue<string>())
                        {
                            case "none":
                                cmbMouseClick.SelectedIndex = 0;
                                break;
                            case "right":
                                cmbMouseClick.SelectedIndex = 1;
                                break;
                            case "middle":
                                cmbMouseClick.SelectedIndex = 2;
                                break;
                            case "left":
                                cmbMouseClick.SelectedIndex = 3;
                                break;
                        }
                    }

                    // Keyboard Key
                    keyboardKeyCode = (_action.Data.ContainsKey("keyboardKeyCode") ? _action.Data["keyboardKeyCode"].GetValue<ushort>() : ushort.MaxValue);

                    if (keyboardKeyCode != ushort.MaxValue)
                    {
                        txtActualKeyboadKey.Text = "Tecla gravada: " + KeyboardController.GetFriendlyKeyNameFromScanCode(keyboardKeyCode);
                    }

                    if (_action.Data.ContainsKey("keyboardKeyAction"))
                    {
                        switch (_action.Data["keyboardKeyAction"].GetValue<string>())
                        {
                            case "press":
                                cmbKeyboardKeyAction.SelectedIndex = 0;
                                break;
                            case "release":
                                cmbKeyboardKeyAction.SelectedIndex = 1;
                                break;
                            case "press/release":
                                cmbKeyboardKeyAction.SelectedIndex = 2;
                                break;
                        }
                    }

                    // Keyboard Text
                    txbKeyboardText.Text = (_action.Data.ContainsKey("keyboardText") ? _action.Data["keyboardText"].GetValue<string>() : "");

                    // Wait Time
                    txbWaitTime.Text = (_action.Data.ContainsKey("waitTime") ? _action.Data["waitTime"].GetValue<uint>().ToString() : "");
                }
            }
        }

        private void MacroKey_FormClosing(object sender, FormClosingEventArgs e)
        {
            _parent.Visible = true;
        }

        private void btnSaveAndClose_Click(object sender, EventArgs e)
        {
            JsonObject json = null;

            if (tglMouse.Checked)
            {
                string mouseClick = "none";

                if (cmbMouseClick.SelectedIndex == 1)
                {
                    mouseClick = "right";
                }
                else if (cmbMouseClick.SelectedIndex == 2)
                {
                    mouseClick = "middle";
                }
                else if (cmbMouseClick.SelectedIndex == 3)
                {
                    mouseClick = "left";
                }


                json = new JsonObject
                {
                    {"mouseX", (txbMouseX.Text.Length > 0 ? int.Parse(txbMouseX.Text) : 0 )},
                    {"mouseY", (txbMouseY.Text.Length > 0 ? int.Parse(txbMouseY.Text) : 0 )},
                    {"mouseClick", mouseClick}
                };

                _action = new Class.App.Input.MacroKey.MacroKeyAction(_idInput, Class.App.Input.MacroKey.MacroKeyAction.ActionType.Mouse, json);
                DialogResult = DialogResult.OK;
                Close();
            }
            else if (tglKeyboardKey.Checked)
            {
                string keyboardKeyAction = "none";

                if (cmbKeyboardKeyAction.SelectedIndex == 0)
                {
                    keyboardKeyAction = "press";
                }
                else if (cmbKeyboardKeyAction.SelectedIndex == 1)
                {
                    keyboardKeyAction = "release";
                }
                else if (cmbKeyboardKeyAction.SelectedIndex == 2)
                {
                    keyboardKeyAction = "press/release";
                }


                json = new JsonObject
                {
                    {"keyboardKeyCode", (ushort) keyboardKeyCode},
                    {"keyboardKeyAction", keyboardKeyAction}
                };

                _action = new Class.App.Input.MacroKey.MacroKeyAction(_idInput, Class.App.Input.MacroKey.MacroKeyAction.ActionType.KeyboardKey, json);
                DialogResult = DialogResult.OK;
                Close();
            }
            else if (tglKeyboardText.Checked)
            {
                json = new JsonObject
                {
                    {"keyboardText", txbKeyboardText.Text}
                };

                _action = new Class.App.Input.MacroKey.MacroKeyAction(_idInput, Class.App.Input.MacroKey.MacroKeyAction.ActionType.KeyboardText, json);
                DialogResult = DialogResult.OK;
                Close();
            }
            else if (tglWaitTime.Checked)
            {
                json = new JsonObject
                {
                    {"waitTime", uint.Parse(txbWaitTime.Text) }
                };

                _action = new Class.App.Input.MacroKey.MacroKeyAction(_idInput, Class.App.Input.MacroKey.MacroKeyAction.ActionType.Wait, json);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnKeyboardKeyRec_Click(object sender, EventArgs e)
        {
            if (_keyboardRecEnabled)
            {
                btnKeyboardKeyRec.Text = "Gravar Tecla";
                _keyboardRecEnabled = false;
            }
            else
            {
                btnKeyboardKeyRec.Text = "Gravando...";
                // Used to unfocus the button
                Focus();
                _keyboardRecEnabled = true;
            }
        }

        private void MacroKeyAction_KeyDown(object sender, KeyEventArgs e)
        {
            if (_keyboardRecEnabled)
            {
                btnKeyboardKeyRec.Text = "Gravar Tecla";
                keyboardKeyCode = KeyboardController.ConvertToScanMode(e.KeyCode);
                txtActualKeyboadKey.Text = "Tecla gravada: " + KeyboardController.GetFriendlyKeyNameFromScanCode((ushort)keyboardKeyCode);
                _keyboardRecEnabled = false;
            }
        }

        private void ChangeWaitTimeInputs(bool canWrite)
        {
            txbWaitTime.Enabled = canWrite;

            if (canWrite)
            {
                DisableOthersInputsScreen("waitTime");
            }
        }

        private void tglWaitTime_CheckedChanged(object sender, EventArgs e)
        {
            ChangeWaitTimeInputs((sender as MaterialSwitch).Checked);
        }

        private void ChangeKeyboardTextInputs(bool canWrite)
        {
            txbKeyboardText.Enabled = canWrite;

            if (canWrite)
            {
                DisableOthersInputsScreen("keyboardText");
            }
        }

        private void tglKeyboardText_CheckedChanged(object sender, EventArgs e)
        {
            ChangeKeyboardTextInputs((sender as MaterialSwitch).Checked);

        }

        private void ChangeKeyboardKeyInputs(bool canWrite)
        {
            btnKeyboardKeyRec.Enabled = canWrite;
            cmbKeyboardKeyAction.Enabled = canWrite;
            txtActualKeyboadKey.Enabled = canWrite;

            if (canWrite)
            {
                DisableOthersInputsScreen("keyboardKey");
            }
        }

        private void tglKeyboardKey_CheckedChanged(object sender, EventArgs e)
        {
            ChangeKeyboardKeyInputs((sender as MaterialSwitch).Checked);
        }

        private void ChangeMouseInputs(bool canWrite)
        {
            txbMouseX.Enabled = canWrite;
            txbMouseY.Enabled = canWrite;
            cmbMouseClick.Enabled = canWrite;

            if (canWrite)
            {
                DisableOthersInputsScreen("mouse");
            }
        }

        private void tglMouse_CheckedChanged(object sender, EventArgs e)
        {
            ChangeMouseInputs((sender as MaterialSwitch).Checked);
        }

        private void DisableOthersInputsScreen(string type)
        {
            if (type != "mouse")
            {
                tglMouse.Checked = false;
            }
 
            if (type != "keyboardKey")
            {
                tglKeyboardKey.Checked = false;
            }

            if (type != "keyboardText")
            {
                tglKeyboardText.Checked = false;
            }

            if (type != "waitTime")
            {
                tglWaitTime.Checked = false;
            }
        }
    }
}
