using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJManager.Class.App.PixelGrid
{
    internal class Pixel
    {
        public Rectangle Bounds { get; set; }
        public bool IsOn { get; set; }
        public bool IsSelected { get; set; }
    }
}
