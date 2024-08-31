using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Drawing.Brush;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using FontStyle = System.Drawing.FontStyle;
using Pen = System.Drawing.Pen;

namespace museumAIS
{
    public class generateCaptcha
    {
        // Свойство для хранения текста капчи
        public string captchaText { get; private set; }

        // Метод для генерации изображения капчи
        public ImageSource GenerateImageCaptcha()
        {
            // Генерация случайного текста капчи
            captchaText = GenerateRandomCaptchaText(4);

            // Генерация изображения капчи на основе текста
            Bitmap captchaImage = GenerateCaptchaImage(captchaText, 120, 50);

            // Конвертация изображения Bitmap в ImageSource для использования в WPF
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHBitmap(
                captchaImage.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(captchaImage.Width, captchaImage.Height));

            return imageSource;
        }

        // Метод для генерации случайного текста капчи заданной длины
        private string GenerateRandomCaptchaText(int length)
        {
            Random random = new Random();
            const string chars = "0abcd1efgh2ijkl3mnop4qr5st6u7v8w9xyz";
            char[] captchaChars = new char[length];

            // Заполнение массива случайными символами
            for (int i = 0; i < length; i++)
            {
                captchaChars[i] = chars[random.Next(chars.Length)];
            }

            // Преобразование массива символов в строку
            return new string(captchaChars);
        }

        // Метод для создания изображения капчи на основе текста
        private Bitmap GenerateCaptchaImage(string text, int width, int height)
        {
            // Создание пустого изображения с заданными размерами
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Инициализация графики для рисования на изображении
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, width, height));

            Random random = new Random();
            Font font = new Font("Arial", 20, FontStyle.Bold);

            // Рисование каждого символа текста на изображении
            for (int i = 0; i < text.Length; i++)
            {
                Brush brush = new SolidBrush(Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)));
                PointF point = new PointF((float)i * width / text.Length + random.Next(-5, 5), random.Next(-5, 5));
                graphics.DrawString(text[i].ToString(), font, brush, point);
            }

            // Рисование случайных линий на изображении для усложнения капчи
            for (int i = 0; i < 10; i++)
            {
                Pen pen = new Pen(Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)), 1);
                int x1 = random.Next(0, width);
                int y1 = random.Next(0, height);
                int x2 = random.Next(0, width);
                int y2 = random.Next(0, height);
                graphics.DrawLine(pen, x1, y1, x2, y2);
            }

            // Добавление случайных пикселей на изображение для усложнения капчи
            for (int i = 0; i < 50; i++)
            {
                bitmap.SetPixel(random.Next(0, width), random.Next(0, height), Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)));
            }

            // Очистка ресурсов
            graphics.Flush();
            font.Dispose();
            graphics.Dispose();

            return bitmap;
        }
    }
}
