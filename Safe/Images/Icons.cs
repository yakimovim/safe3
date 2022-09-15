using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EdlinSoftware.Safe.Domain;

namespace EdlinSoftware.Safe.Images;

public static class Icons
{
    private static ImageSource? _defaultItemIcon;

    public static ImageSource DefaultItemIcon
    {
        get
        {
            if (_defaultItemIcon == null)
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("EdlinSoftware.Safe.Images.globe16.png");
                image.EndInit();
                _defaultItemIcon = image;
            }

            return _defaultItemIcon;
        }
    }

    public static ImageSource GetIcon(this IIconsRepository iconsRepository, string? iconId)
    {
        if (iconId == null) return DefaultItemIcon;

        var icon = iconsRepository.GetIconById(iconId);

        if (icon == null) return DefaultItemIcon;

        var image = new BitmapImage();
        image.BeginInit();
        image.StreamSource = icon.GetStream();
        image.EndInit();

        return image;
    }
}