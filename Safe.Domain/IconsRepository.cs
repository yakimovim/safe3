using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EdlinSoftware.Safe.Domain.Model;
using IStorageIconsRepository = EdlinSoftware.Safe.Storage.IIconsRepository;

namespace EdlinSoftware.Safe.Domain
{
    public class IconsRepository : IIconsRepository
    {
        private readonly IStorageIconsRepository _iconsRepository;

        public IconsRepository(IStorageIconsRepository iconsRepository)
        {
            _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));
        }

        public string CreateNewIcon(Stream iconStream) => _iconsRepository.CreateNewIcon(iconStream);

        public IEnumerable<Icon> GetAllIcons()
        {
            return _iconsRepository.GetAllIcons()
                .Select(i => new Icon(i));
        }

        public Icon? GetIconById(string id)
        {
            var storageIcon = _iconsRepository.GetIconById(id);

            return storageIcon == null 
                ? null 
                : new Icon(storageIcon);
        }

        public void DeleteIcon(string id) => _iconsRepository.DeleteIcon(id);
    }
}