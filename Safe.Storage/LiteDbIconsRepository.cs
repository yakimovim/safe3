using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EdlinSoftware.Safe.Storage.Model;
using LiteDB;

namespace EdlinSoftware.Safe.Storage
{
    public class LiteDbIconsRepository : IIconsRepository
    {
        private readonly ILiteDbConnectionProvider _dbProvider;

        private ILiteStorage<string> Storage => _dbProvider.GetDatabase().FileStorage;

        public LiteDbIconsRepository(ILiteDbConnectionProvider dbProvider)
        {
            _dbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
        }

        public string CreateNewIcon(Stream iconStream)
        {
            var id = Guid.NewGuid().ToString("N");

            Storage.Upload(id, id, iconStream);

            return id;
        }

        public IEnumerable<Icon> GetAllIcons()
        {
            return Storage.FindAll()
                .Select(i => new Icon(i.Id, i.OpenRead));
        }

        public Icon? GetIconById(string id)
        {
            var file = Storage.FindById(id);

            if (file == null) return null;

            return new Icon(file.Id, file.OpenRead);
        }

        public void DeleteIcon(string id)
        {
            Storage.Delete(id);

            var itemsCollection = _dbProvider.GetDatabase().GetCollection<Item>();

            var itemsWithIcon = itemsCollection
                .Find(i => i.IconId == id)
                .ToArray();

            foreach (var item in itemsWithIcon)
            {
                item.IconId = null;
            }

            itemsCollection.Update(itemsWithIcon);
        }
    }
}