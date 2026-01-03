using MaterialSkin;
using MaterialSkin.Controls;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;

namespace JJManager.Pages.App
{
    public partial class MessageBox : MaterialForm
    {
        private MaterialForm _parent = null;
        //private string _title = string.Empty;
        //private string _message = string.Empty;
        //private MessageBoxButtons _button;

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        #endregion

        public MessageBox(MaterialForm parent, string title, string message, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            InitializeComponent();
            components = new System.ComponentModel.Container();

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            _parent = parent;

            Text = title;
            TxtMessageBox.Text = message;

            if (buttons == MessageBoxButtons.OK)
            {
                MaterialButton btnOk = new MaterialButton
                {
                    Text = "OK",
                    Name = "BtnOk"
                };

                btnOk.Click += BtnOk_Click;

                FlpButtons.Controls.Add(btnOk);
            }

            if (buttons == MessageBoxButtons.OKCancel)
            {
                MaterialButton btnOk = new MaterialButton
                {
                    Text = "OK",
                    Name = "BtnOk"
                };

                MaterialButton btnCancel = new MaterialButton
                {
                    Text = "Cancelar",
                    Name = "BtnCancel"
                };

                btnOk.Click += BtnOk_Click;
                btnCancel.Click += BtnCancel_Click;

                FlpButtons.Controls.Add(btnOk);
                FlpButtons.Controls.Add(btnCancel);
            }

            if (buttons == MessageBoxButtons.YesNo)
            {
                MaterialButton btnYes = new MaterialButton
                {
                    Text = "Sim",
                    Name = "BtnYes"
                };

                MaterialButton btnNo = new MaterialButton
                {
                    Text = "Não",
                    Name = "BtnNo"
                };

                btnYes.Click += BtnYes_Click;
                btnNo.Click += BtnNo_Click;

                FlpButtons.Controls.Add(btnYes);
                FlpButtons.Controls.Add(btnNo);
            }

            if (buttons == MessageBoxButtons.YesNoCancel)
            {
                MaterialButton btnYes = new MaterialButton
                {
                    Text = "Sim",
                    Name = "BtnYes"
                };

                MaterialButton btnNo = new MaterialButton
                {
                    Text = "Não",
                    Name = "BtnNo"
                };

                MaterialButton btnCancel = new MaterialButton
                {
                    Text = "Cancelar",
                    Name = "BtnCancel"
                };

                btnYes.Click += BtnYes_Click;
                btnNo.Click += BtnNo_Click;
                btnCancel.Click += BtnCancel_Click;

                FlpButtons.Controls.Add(btnYes);
                FlpButtons.Controls.Add(btnNo);
                FlpButtons.Controls.Add(btnCancel);
            }
        }

        private void BtnNo_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }

        private void BtnYes_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void BtnCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnOk_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        public static DialogResult Show(MaterialForm parent, string title, string message, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            MaterialForm safeForm = parent ?? Application.OpenForms.Cast<MaterialForm>().LastOrDefault();

            if (safeForm != null && safeForm.InvokeRequired)
            {
                DialogResult result = DialogResult.None;

                safeForm.Invoke((MethodInvoker)(() =>
                    result = ShowInternal(safeForm, title, message, buttons)
                ));

                return result;
            }
            else
            {
                return ShowInternal(safeForm, title, message, buttons);
            }
        }

        public static async Task ShowAsync(MaterialForm parent, string title, string message, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            MaterialForm safeForm = parent ?? Application.OpenForms.Cast<MaterialForm>().FirstOrDefault();

            if (safeForm != null && safeForm.InvokeRequired)
            {
                await Task.Run(() => safeForm.Invoke((MethodInvoker)(() =>
                    ShowInternal(safeForm, title, message, buttons)
                )));
            }
            else
            {
                ShowInternal(safeForm, title, message, buttons);
            }
        }

        private static DialogResult ShowInternal(MaterialForm parent, string title, string message, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            MessageBox box = new MessageBox(parent, title, message, buttons);

            if (parent != null)
            {
                box.BringToFront();
                // Center the new MessageBox form on the parent form
                box.StartPosition = FormStartPosition.Manual;
                box.Location = new System.Drawing.Point(
                    parent.Left + (parent.Width - box.Width) / 2,
                    parent.Top + (parent.Height - box.Height) / 2
                );
                return box.ShowDialog(parent); // stays open until user closes
            }
            else
            {
                box.StartPosition = FormStartPosition.CenterScreen;
                box.BringToFront();
                return box.ShowDialog(); // fallback without parent
            }

        }
    }
}
