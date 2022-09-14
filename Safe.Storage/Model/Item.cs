using System.Collections.Generic;

namespace EdlinSoftware.Safe.Storage.Model
{
    public class Item
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public string? IconId { get; set; }

        public List<Field> Fields { get; set; } = new List<Field>();
    }
}