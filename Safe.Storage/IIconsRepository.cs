using System.Collections.Generic;
using System.IO;
using EdlinSoftware.Safe.Storage.Model;

namespace EdlinSoftware.Safe.Storage
{
    public interface IIconsRepository
    {
        string CreateNewIcon(Stream iconStream);

        IEnumerable<Icon> GetAllIcons();

        Icon? GetIconById(string id);

        void DeleteIcon(string id);
    }
}