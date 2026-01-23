namespace JJManager.Pages.App
{
    partial class UpdateAppNotification
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            // Dispose WebView2 explicitly
            if (webView2Changelog != null)
            {
                webView2Changelog.Dispose();
                webView2Changelog = null;
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtPresentation01 = new MaterialSkin.Controls.MaterialLabel();
            this.btnGoToUpdateScreen = new MaterialSkin.Controls.MaterialButton();
            this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            this.webView2Changelog = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webView2Changelog)).BeginInit();
            this.SuspendLayout();
            //
            // txtPresentation01
            //
            this.txtPresentation01.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPresentation01.Depth = 0;
            this.txtPresentation01.Font = new System.Drawing.Font("Roboto Medium", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.txtPresentation01.FontType = MaterialSkin.MaterialSkinManager.fontType.H6;
            this.txtPresentation01.Location = new System.Drawing.Point(6, 90);
            this.txtPresentation01.MouseState = MaterialSkin.MouseState.HOVER;
            this.txtPresentation01.Name = "txtPresentation01";
            this.txtPresentation01.Size = new System.Drawing.Size(848, 29);
            this.txtPresentation01.TabIndex = 0;
            this.txtPresentation01.Text = "É com grande orgulho que lançamos oficialmente para todos uma nova versão do JJMa" +
    "nager!";
            this.txtPresentation01.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // btnGoToUpdateScreen
            //
            this.btnGoToUpdateScreen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGoToUpdateScreen.AutoSize = false;
            this.btnGoToUpdateScreen.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnGoToUpdateScreen.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.btnGoToUpdateScreen.Depth = 0;
            this.btnGoToUpdateScreen.HighEmphasis = true;
            this.btnGoToUpdateScreen.Icon = null;
            this.btnGoToUpdateScreen.Location = new System.Drawing.Point(305, 586);
            this.btnGoToUpdateScreen.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnGoToUpdateScreen.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnGoToUpdateScreen.Name = "btnGoToUpdateScreen";
            this.btnGoToUpdateScreen.NoAccentTextColor = System.Drawing.Color.Empty;
            this.btnGoToUpdateScreen.Size = new System.Drawing.Size(250, 51);
            this.btnGoToUpdateScreen.TabIndex = 1;
            this.btnGoToUpdateScreen.Text = "Ir para a aba de atualização";
            this.btnGoToUpdateScreen.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnGoToUpdateScreen.UseAccentColor = false;
            this.btnGoToUpdateScreen.UseVisualStyleBackColor = true;
            this.btnGoToUpdateScreen.Click += new System.EventHandler(this.btnGoToUpdateScreen_Click);
            //
            // materialLabel1
            //
            this.materialLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialLabel1.Depth = 0;
            this.materialLabel1.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel1.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle1;
            this.materialLabel1.Location = new System.Drawing.Point(7, 130);
            this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel1.Name = "materialLabel1";
            this.materialLabel1.Size = new System.Drawing.Size(848, 23);
            this.materialLabel1.TabIndex = 2;
            this.materialLabel1.Text = "Abaixo você pode conferir algumas novidades da versão.";
            this.materialLabel1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // webView2Changelog
            //
            this.webView2Changelog.AllowExternalDrop = false;
            this.webView2Changelog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webView2Changelog.CreationProperties = null;
            this.webView2Changelog.DefaultBackgroundColor = System.Drawing.Color.Transparent;
            this.webView2Changelog.Location = new System.Drawing.Point(7, 163);
            this.webView2Changelog.Name = "webView2Changelog";
            this.webView2Changelog.Size = new System.Drawing.Size(846, 413);
            this.webView2Changelog.TabIndex = 3;
            this.webView2Changelog.ZoomFactor = 1D;
            //
            // UpdateAppNotification
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 650);
            this.Controls.Add(this.webView2Changelog);
            this.Controls.Add(this.materialLabel1);
            this.Controls.Add(this.btnGoToUpdateScreen);
            this.Controls.Add(this.txtPresentation01);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(860, 450);
            this.Name = "UpdateAppNotification";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nova Atualização: ";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.webView2Changelog)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private MaterialSkin.Controls.MaterialLabel txtPresentation01;
        private MaterialSkin.Controls.MaterialButton btnGoToUpdateScreen;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView2Changelog;
    }
}