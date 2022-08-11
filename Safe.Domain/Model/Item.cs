using System;
using System.Collections.Generic;

namespace EdlinSoftware.Safe.Domain.Model
{
    public sealed class Item
    {
        public Item(Item? parentItem = null)
        {
            ParentId = parentItem?.Id;

            if (ParentId == 0)
                throw new ArgumentException("Save parent item before creation of child items");
        }

        internal int Id { get; set; }

        internal int? ParentId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public List<Field> Fields { get; } = new List<Field>();
    }
}