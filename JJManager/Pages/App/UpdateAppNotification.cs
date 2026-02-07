using JJManager.Class;
using JJManager.Class.App;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConfigClass = JJManager.Class.App.Config.Config;

namespace JJManager.Pages.App
{
    public partial class UpdateAppNotification : MaterialForm
    {
        JJManager.Class.App.SoftwareUpdater _softwareUpdater = null;
        private bool _webViewInitialized = false;
        private Task _webViewInitTask;
        MaterialForm _parent = null;

        #region WinForms
        private MaterialSkinManager materialSkinManager = null;
        #endregion

        public UpdateAppNotification(JJManager.Class.App.SoftwareUpdater softwareUpdater)
        {
            DatabaseConnection database = new DatabaseConnection();

            InitializeComponent();
            components = new System.ComponentModel.Container();

            _softwareUpdater = softwareUpdater;
            Text += _softwareUpdater.LastVersion;

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            // Initialize and load changelog
            Shown += async (_, __) =>
            {
                await EnsureWebViewInitializedAsync();
                await LoadChangelogHtml();
            };

            FormClosing += UpdateAppNotification_FormClosing;
        }

        private void UpdateAppNotification_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseUpdateAppNotification();
        }

        public UpdateAppNotification(MaterialForm parent, JJManager.Class.App.SoftwareUpdater softwareUpdater)
        {
            DatabaseConnection database = new DatabaseConnection();
            _parent = parent;

            InitializeComponent();
            components = new System.ComponentModel.Container();

            _softwareUpdater = softwareUpdater;
            Text += _softwareUpdater.LastVersion;

            // MaterialDesign
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = ConfigClass.Theme.SelectedTheme;
            materialSkinManager.ColorScheme = ConfigClass.Theme.SelectedColorScheme;

            // Initialize and load changelog
            Shown += async (_, __) =>
            {
                await EnsureWebViewInitializedAsync();
                await LoadChangelogHtml();
            };

            FormClosing += UpdateAppNotification_FormClosing;

            StartPosition = FormStartPosition.CenterParent;
            Focus();

            if (_parent != null)
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(_parent.Location.X + _parent.Width / 2 - Width / 2,
                    _parent.Location.Y + _parent.Height / 2 - Height / 2);
            }
            else
            {
                StartPosition = FormStartPosition.CenterParent;
            }
        }

        /// <summary>
        /// Initializes the WebView2 control and loads the changelog HTML
        /// </summary>
        private async Task InitializeWebViewAsync()
        {
            if (_webViewInitialized)
                return;

            _webViewInitialized = true;

            try
            {
                if (webView2Changelog.CoreWebView2 == null)
                {
                    await webView2Changelog.EnsureCoreWebView2Async();
                }

                await LoadChangelogHtml();
            }
            catch (Exception ex)
            {
                Log.Insert("UpdateAppNotification", "Erro ao inicializar WebView2", ex);

                // Fallback: Show error message in the webview
                try
                {
                    string errorHtml = $@"
<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'></head>
<body style='font-family: Roboto, sans-serif; padding: 20px; text-align: center;'>
    <h2>Erro ao carregar changelog</h2>
    <p>{ex.Message}</p>
</body>
</html>";
                    webView2Changelog.NavigateToString(errorHtml);
                }
                catch
                {
                    // If even the error message fails, just log it
                }
            }
        }

        private async Task EnsureWebViewInitializedAsync()
        {
            if (_webViewInitTask != null)
                return;

            _webViewInitTask = InitializeInternalAsync();
            await _webViewInitTask;
        }

        private async Task InitializeInternalAsync()
        {
            var env = await HtmlTemplateEngine.GetAsync();
            await webView2Changelog.EnsureCoreWebView2Async(env);
        }

        /// <summary>
        /// Generates and loads the changelog HTML into WebView2
        /// </summary>
        private async Task LoadChangelogHtml()
        {
            try
            {
                // Determine if dark theme is active
                bool isDarkTheme = materialSkinManager.Theme == MaterialSkinManager.Themes.DARK;

                // Generate HTML from changelog data
                string html = Class.App.HtmlChangelogGenerator.GenerateHtml(
                    _softwareUpdater.ChangeLog,
                    isDarkTheme,
                    materialSkinManager.ColorScheme
                );

                await EnsureWebViewInitializedAsync();
                
                // Load HTML into WebView2
                webView2Changelog.NavigateToString(html);

                // Wait for navigation to complete
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                Log.Insert("UpdateAppNotification", "Erro ao carregar HTML no WebView2", ex);
                throw;
            }
        }

        private void btnGoToUpdateScreen_Click(object sender, EventArgs e)
        {
            Main parentMain = (Main)_parent;
            // Do whatever you need with the desired child form
            // For example:
            parentMain.GoToUpdateTab();

            Close();
        }

        public void CloseUpdateAppNotification()
        {
            _parent.Visible = true;
            DialogResult = DialogResult.OK;
        }
    }
}
