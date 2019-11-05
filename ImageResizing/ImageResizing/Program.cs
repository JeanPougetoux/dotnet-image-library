using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageResizing
{
    class Program
    {
        private static Bitmap MergedBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            Bitmap result = new Bitmap(Math.Max(bmp1.Width, bmp2.Width),
                                       bmp1.Height + bmp2.Height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp1, new Point(0, 0));
                g.DrawImage(bmp2, new Point(0, bmp2.Height));
            }
            return result;
        }

        static void Main(string[] args)
        {
            var access = @"C:\Users\formation\Pictures\dracaufeu.jpg";

            Bitmap image = new Bitmap(access);

            var width = image.Width;
            var height = image.Height;

            var pixelsSpan = height / 100 * 12;

            var cropTopArea = new Rectangle(0, 0, width, pixelsSpan);
            var cropBottomArea = new Rectangle(0, height - pixelsSpan, width, pixelsSpan);

            Bitmap top = image.Clone(cropTopArea,
            image.PixelFormat);

            Bitmap bottom = image.Clone(cropBottomArea,
            image.PixelFormat);

            Bitmap final = MergedBitmaps(top, bottom);
            final.Save(@"C:\Users\formation\Pictures\dracaufeuresized.jpg");
        }
    }
}
