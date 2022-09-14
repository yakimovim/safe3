using System.IO;
using StorageIcon = EdlinSoftware.Safe.Storage.Model.Icon;

namespace EdlinSoftware.Safe.Domain.Model
{
    public class Icon
    {
        private readonly StorageIcon _icon;

        public Icon(StorageIcon icon)
        {
            _icon = icon;
        }

        public string Id => _icon.Id;

        public Stream GetStream() => _icon.GetStream();
    }
}