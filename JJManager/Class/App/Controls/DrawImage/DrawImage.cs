using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace JJManager.Class.App.Controls.DrawImage
{
    public partial class DrawImage : Control
    {
        private readonly Timer refresher;
        private Image _image;
        private float aspectRatio = 1f; // Default aspect ratio (will be updated when an image is set)

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Image Image
        {
            get => _image;
            set
            {
                _image = value;
                RecreateHandle();
                Invalidate(); // Refresh control
            }
        }

        // Set a placeholder image in design mode
        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (DesignMode && _image == null)
            {
                //_image = new Bitmap(Width, Height);
                //using (Graphics g = Graphics.FromImage(_image))
                //{
                //    g.Clear(Color.LightGray);
                //    g.DrawString("Image Placeholder", Font, Brushes.Black, new PointF(10, 10));
                //}
            }
        }

        public DrawImage()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            
            if (DesignMode)
            {
                return; // Don't initialize the timer in design mode
            }

            refresher = new Timer();
            refresher.Tick += TimerOnTick;
            refresher.Interval = 50;
            refresher.Enabled = true;
            refresher.Start();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }

        protected override void OnMove(EventArgs e)
        {
            RecreateHandle();
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime && _image != null)
            {
                // Maintain aspect ratio in design mode
                if (width != Width)
                {
                    height = (int)(width / aspectRatio);
                }
                else if (height != Height)
                {
                    width = (int)(height * aspectRatio);
                }
            }

            base.SetBoundsCore(x, y, width, height, specified);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_image != null) // Enables rendering in Visual Studio Designer
            {
                aspectRatio = (float)_image.Width / _image.Height;
                DrawScaledImage(e.Graphics);

                //float imgRatio = (float)_image.Width / _image.Height;
                //float controlRatio = (float)Width / Height;

                //int drawWidth, drawHeight;

                //if (imgRatio > controlRatio)
                //{
                //    drawWidth = Width;
                //    drawHeight = (int)(Width / imgRatio);
                //}
                //else
                //{
                //    drawHeight = Height;
                //    drawWidth = (int)(Height * imgRatio);
                //}

                //int x = (Width - drawWidth) / 2;
                //int y = (Height - drawHeight) / 2;

                //e.Graphics.DrawImage(_image, x, y, drawWidth, drawHeight);
            }
        }

        private void DrawScaledImage(Graphics g)
        {
            if (_image == null) return;

            float imgRatio = (float)_image.Width / _image.Height;
            float controlRatio = (float)Width / Height;

            int drawWidth, drawHeight;

            if (imgRatio > controlRatio)
            {
                drawWidth = Width;
                drawHeight = (int)(Width / imgRatio);
            }
            else
            {
                drawHeight = Height;
                drawWidth = (int)(Height * imgRatio);
            }

            int x = (Width - drawWidth) / 2;
            int y = (Height - drawHeight) / 2;

            g.DrawImage(_image, x, y, drawWidth, drawHeight);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate(); // Redraw when resized
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //Do not paint background
        }

        //Hack
        public void Redraw()
        {
            RecreateHandle();
        }

        private void TimerOnTick(object source, EventArgs e)
        {
            RecreateHandle();
            refresher.Stop();
        }
    }
}