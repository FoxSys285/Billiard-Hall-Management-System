using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace QuanLyQuanBida.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var path = value.ToString();
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                // Absolute URI or file path
                if (Uri.TryCreate(path, UriKind.Absolute, out Uri uri))
                {
                    if (uri.IsFile && File.Exists(uri.LocalPath))
                        return CreateBitmap(uri);

                    return CreateBitmap(uri);
                }

                // Try file path relative to executable folder
                var appPath = AppDomain.CurrentDomain.BaseDirectory;
                var relativePath = path.TrimStart('\\', '/');
                var combined = Path.Combine(appPath, relativePath);
                if (File.Exists(combined))
                    return CreateBitmap(new Uri(combined));

                // If value is just a filename (e.g., "sting.jpg" from DB), try Images/<filename>
                var imagesCombined = Path.Combine(appPath, "Images", relativePath);
                if (File.Exists(imagesCombined))
                    return CreateBitmap(new Uri(imagesCombined));

                // Try pack URI for content resource (first with Images/, then raw)
                var assemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "QuanLyQuanBida";
                var normalizedImages = ("Images/" + relativePath).Replace("\\", "/");
                var packUriImages = new Uri($"pack://application:,,,/{assemblyName};component/{normalizedImages}", UriKind.Absolute);
                try { return CreateBitmap(packUriImages); } catch { }

                var normalized = relativePath.Replace("\\", "/");
                var packUri = new Uri($"pack://application:,,,/{assemblyName};component/{normalized}", UriKind.Absolute);
                return CreateBitmap(packUri);
            }
            catch
            {
                return null;
            }
        }

        private BitmapImage CreateBitmap(Uri uri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = uri;
            bitmap.EndInit();
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
