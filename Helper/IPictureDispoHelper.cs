using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SplittableDataGridSAmple.Helper
{
    class IPictureDispoHelper
    {
    }

    internal class OleCreateConverter
    {

        [DllImport("oleaut32.dll", EntryPoint = "OleCreatePictureIndirect", CharSet = CharSet.Ansi, ExactSpelling = true, PreserveSig = true)]
        private static extern int OleCreatePictureIndirect(
            [In] PictDescBitmap pictdesc, ref Guid iid, bool fOwn,
            [MarshalAs(UnmanagedType.Interface)] out object ppVoid);

        const short _PictureTypeBitmap = 1;
        [StructLayout(LayoutKind.Sequential)]
        internal class PictDescBitmap
        {
            internal int cbSizeOfStruct = Marshal.SizeOf(typeof(PictDescBitmap));
            internal int pictureType = _PictureTypeBitmap;
            internal IntPtr hBitmap = IntPtr.Zero;
            internal IntPtr hPalette = IntPtr.Zero;
            internal int unused = 0;

            internal PictDescBitmap(Bitmap bitmap)
            {
                this.hBitmap = bitmap.GetHbitmap();
            }
        }

        public static stdole.IPictureDisp ImageToPictureDisp(Image image)
        {
            if (image == null || !(image is Bitmap))
            {
                return null;
            }

            PictDescBitmap pictDescBitmap = new PictDescBitmap((Bitmap)image);
            object ppVoid = null;
            Guid iPictureDispGuid = typeof(stdole.IPictureDisp).GUID;
            OleCreatePictureIndirect(pictDescBitmap, ref iPictureDispGuid, true, out ppVoid);
            stdole.IPictureDisp picture = (stdole.IPictureDisp)ppVoid;
            return picture;
        }

        public static Image PictureDispToImage(stdole.IPictureDisp pictureDisp)
        {
            Image image = null;
            if (pictureDisp != null && pictureDisp.Type == _PictureTypeBitmap)
            {
                IntPtr paletteHandle = new IntPtr(pictureDisp.hPal);
                IntPtr bitmapHandle = new IntPtr(pictureDisp.Handle);
                image = Image.FromHbitmap(bitmapHandle, paletteHandle);
            }
            return image;
        }
    }
}
