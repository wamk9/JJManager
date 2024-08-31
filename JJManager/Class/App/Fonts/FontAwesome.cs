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

        private static void LoadFontAwesomeFont(byte[] fontAwesomeResource)
        {
            // Retrieve the font resource stream
            IntPtr fontData = Marshal.AllocCoTaskMem(fontAwesomeResource.Length);
            Marshal.Copy(fontAwesomeResource, 0, fontData, fontAwesomeResource.Length);

            // Add the font to the private collection
            fontCollection.AddMemoryFont(fontData, fontAwesomeResource.Length);

            // Clean up
            Marshal.FreeCoTaskMem(fontData);
        }

        public static Font UseSolid(int size)
        {
            LoadFontAwesomeFont(Properties.Resources.Font_Awesome_6_Free_Solid_900);

            // Use the font in your controls, e.g., a DataGridView button
            Font fontAwesome = new Font(fontCollection.Families[0], size); // Adjust size as necessary

            return fontAwesome;
        }

        public static Font UseBrands(int size)
        {
            LoadFontAwesomeFont(Properties.Resources.Font_Awesome_6_Brands_Regular_400);

            // Use the font in your controls, e.g., a DataGridView button
            Font fontAwesome = new Font(fontCollection.Families[0], size); // Adjust size as necessary

            return fontAwesome;
        }

        public static Font UseRegular(int size)
        {
            LoadFontAwesomeFont(Properties.Resources.Font_Awesome_6_Free_Regular_400);

            // Use the font in your controls, e.g., a DataGridView button
            Font fontAwesome = new Font(fontCollection.Families[0], size); // Adjust size as necessary

            return fontAwesome;
        }
    }
}
