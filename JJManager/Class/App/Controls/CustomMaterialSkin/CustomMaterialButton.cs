using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin.Controls;

namespace JJManager.Class.App.Controls.CustomMaterialSkin
{
    public partial class CustomMaterialButton : MaterialButton
    {
        private string _customText = string.Empty;

        public override string Text
        {
            get => _customText;
            set
            {
                _customText = value;
                Invalidate(); // triggers repaint
            }
        }

        public CustomMaterialButton()
        {
            InitializeComponent();
            Text = string.Empty;
        }

        public CustomMaterialButton(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            Text = string.Empty;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); // Avoids drawing Roboto text

            TextRenderer.DrawText(
                e.Graphics,
                _customText,
                this.Font,
                this.ClientRectangle,
                this.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }
    }
}