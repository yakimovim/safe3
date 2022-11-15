using System;
using System.Collections.Generic;

namespace EdlinSoftware.Safe.Domain.Model
{
    public sealed class Item
    {
        public Item(Item? parentItem = null)
        {
            MoveTo(parentItem);
        }

        internal int Id { get; set; }

        internal int? ParentId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? IconId { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public List<Field> Fields { get; } = new List<Field>();

        public void MoveTo(Item? parentItem)
        {
            var parentId = parentItem?.Id;

            if (parentId == 0)
                throw new ArgumentException("Save parent item before creation of child items");

            ParentId = parentId;
        }

        public override bool Equals(object? obj)
        {
            var anotherItem = obj as Item;

            if (anotherItem == null) return false;

            return Id == anotherItem.Id;
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id;
        }

        public static bool operator ==(Item? a, Item? b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Equals(b);
        }

        public static bool operator !=(Item? a, Item? b) => !(a == b);
    }
}