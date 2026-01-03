using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager.Class.App.Controls.CustomMaterialSkin
{
    public partial class CustomMaterialLabel : MaterialLabel
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

        public CustomMaterialLabel()
        {
            InitializeComponent();
            Text = string.Empty;
        }

        public CustomMaterialLabel(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            Text = string.Empty;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaintBackground(e); // Avoids drawing Roboto text

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
