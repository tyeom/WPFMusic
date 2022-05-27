using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Common.Helper;

public class ImageHelper
{
    public static BitmapImage? CreateBitmapImage(string imgFullPath, int decodePixelWidth = 300)
    {
        string imageFileName = Path.GetFileName(imgFullPath);
        if (File.Exists($"{PathHelper.GetLocalImagesDirectory()}{imageFileName}") == false)
            return null;

        BitmapImage bitmapImg = new BitmapImage();
        bitmapImg.BeginInit();
        bitmapImg.CacheOption = BitmapCacheOption.OnDemand;
        bitmapImg.CreateOptions = BitmapCreateOptions.DelayCreation;
        bitmapImg.DecodePixelWidth = decodePixelWidth;
        bitmapImg.UriSource = new Uri($"{PathHelper.GetLocalImagesDirectory()}{imageFileName}");
        bitmapImg.EndInit();

        return bitmapImg;
    }

    public static BitmapImage CreateBitmapImageByRecorce(string fileName, int decodePixelWidth = 300)
    {
        BitmapImage bitmapImg = new BitmapImage();
        bitmapImg.BeginInit();
        bitmapImg.CacheOption = BitmapCacheOption.OnDemand;
        bitmapImg.CreateOptions = BitmapCreateOptions.DelayCreation;
        bitmapImg.DecodePixelWidth = decodePixelWidth;
        bitmapImg.UriSource = new Uri($"/Images/{fileName}", UriKind.Relative);
        bitmapImg.EndInit();

        return bitmapImg;
    }

    public static bool SetImageForImage(Image imageControl, string imgFullPath, bool isLocalDownloadFile = true, int decodePixelWidth = 300)
    {
        BitmapImage bitmapImg = null;
        if (isLocalDownloadFile)
        {
            bitmapImg = CreateBitmapImage(imgFullPath, decodePixelWidth);
        }
        else
        {
            bitmapImg = new BitmapImage();
            bitmapImg.BeginInit();
            bitmapImg.CacheOption = BitmapCacheOption.OnDemand;
            bitmapImg.CreateOptions = BitmapCreateOptions.DelayCreation;
            bitmapImg.DecodePixelWidth = decodePixelWidth;
            bitmapImg.UriSource = new Uri(imgFullPath, UriKind.RelativeOrAbsolute);
            bitmapImg.EndInit();
        }
        if (bitmapImg == null)
        {
            if (string.IsNullOrWhiteSpace(imgFullPath))
                return false;

            imageControl.Source = new BitmapImage(new Uri(imgFullPath));
            return true;
        }
        else
        {
            imageControl.Source = bitmapImg;
            return true;
        }
    }

    public static BitmapImage? Base64ToBitmapImage(string base64Img)
    {
        if (string.IsNullOrWhiteSpace(base64Img))
            return null;

        byte[] binaryData = Convert.FromBase64String(base64Img);

        BitmapImage bitmapImg = new BitmapImage();
        bitmapImg.BeginInit();
        bitmapImg.StreamSource = new MemoryStream(binaryData);
        bitmapImg.EndInit();

        return bitmapImg;
    }

    public static Color GetAverageColor(BitmapSource bitmap)
    {
        var format = bitmap.Format;

        if (format != PixelFormats.Bgr24 &&
            format != PixelFormats.Bgr32 &&
            format != PixelFormats.Bgra32 &&
            format != PixelFormats.Pbgra32)
        {
            throw new InvalidOperationException("BitmapSource must have Bgr24, Bgr32, Bgra32 or Pbgra32 format");
        }

        var width = bitmap.PixelWidth;
        var height = bitmap.PixelHeight;
        var numPixels = width * height;
        var bytesPerPixel = format.BitsPerPixel / 8;
        var pixelBuffer = new byte[numPixels * bytesPerPixel];

        bitmap.CopyPixels(pixelBuffer, width * bytesPerPixel, 0);

        long blue = 0;
        long green = 0;
        long red = 0;

        for (int i = 0; i < pixelBuffer.Length; i += bytesPerPixel)
        {
            blue += pixelBuffer[i];
            green += pixelBuffer[i + 1];
            red += pixelBuffer[i + 2];
        }

        return Color.FromRgb((byte)(red / numPixels), (byte)(green / numPixels), (byte)(blue / numPixels));
    }
}
