using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace deteckt.Extensions
{
    static class BitmapHelper
    {
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            var convertedBitmapImage = new BitmapImage();
            convertedBitmapImage.BeginInit();
            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Bmp);
            memoryStream.Seek(0, SeekOrigin.Begin);
            convertedBitmapImage.StreamSource = memoryStream;
            convertedBitmapImage.EndInit();
            return convertedBitmapImage;
        }
    }
}
