using System.Windows;
using System.Windows.Media;
using EdlinSoftware.Safe.Domain;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

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
                _defaultItemIcon = (ImageSource) Application.Current.FindResource("GlobeDrawingImage");
            }

            return _defaultItemIcon;
        }
    }

    private static readonly FileSvgReader SvgConverter = new FileSvgReader(new WpfDrawingSettings
    {
        IncludeRuntime = false,
        TextAsGeometry = false
    });

    public static ImageSource GetIcon(this IIconsRepository iconsRepository, string? iconId)
    {
        if (iconId == null) return DefaultItemIcon;

        var icon = iconsRepository.GetIconById(iconId);

        if (icon == null) return DefaultItemIcon;

        try
        {
            var drawingGroup = SvgConverter.Read(icon.GetStream());

            var image = new DrawingImage(drawingGroup);

            return image;
        }
        catch
        {
            return DefaultItemIcon;
        }
    }
}