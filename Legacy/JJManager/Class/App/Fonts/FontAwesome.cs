using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager.Class.App.Fonts
{
    public class FontAwesome
    {
        private static PrivateFontCollection fontCollection = new PrivateFontCollection();
        private static FontFamily solidFont;
        private static FontFamily regularFont;
        private static FontFamily brandsFont;
        private static void LoadFontAwesomeFont(ref FontFamily target, byte[] fontAwesomeResource)
        {
            if (target != null)
                return;

            IntPtr fontData = Marshal.AllocCoTaskMem(fontAwesomeResource.Length);
            Marshal.Copy(fontAwesomeResource, 0, fontData, fontAwesomeResource.Length);
            fontCollection.AddMemoryFont(fontData, fontAwesomeResource.Length);
            Marshal.FreeCoTaskMem(fontData);

            // Assign the latest loaded font family to the target reference
            target = fontCollection.Families[fontCollection.Families.Length - 1];
        }

        public static Font UseSolid(int size)
        {
            LoadFontAwesomeFont(ref solidFont, Properties.Resources.fa_solid_900);
            return new Font(solidFont, size, FontStyle.Regular);
        }

        public static Font UseBrands(int size)
        {
            LoadFontAwesomeFont(ref brandsFont, Properties.Resources.fa_brands_400);
            return new Font(brandsFont, size, FontStyle.Regular);
        }

        public static Font UseRegular(int size)
        {
            LoadFontAwesomeFont(ref regularFont, Properties.Resources.fa_regular_400);
            return new Font(regularFont, size, FontStyle.Regular);
        }
    }
}
