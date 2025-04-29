namespace EkzamenADO.Converters
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string filename && !string.IsNullOrEmpty(filename))
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", filename);
                if (File.Exists(path))
                {
                    return new BitmapImage(new Uri(path));
                }
            }

            // если файла нет то пустая картинка
            return new BitmapImage(new Uri("pack://application:,,,/Images/no-image.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
