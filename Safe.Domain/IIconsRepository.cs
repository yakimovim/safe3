using System.Collections.Generic;
using System.IO;
using EdlinSoftware.Safe.Domain.Model;

namespace EdlinSoftware.Safe.Domain
{
    public interface IIconsRepository
    {
        string CreateNewIcon(Stream iconStream);

        IEnumerable<Icon> GetAllIcons();

        Icon? GetIconById(string id);

        void DeleteIcon(string id);

    }
}