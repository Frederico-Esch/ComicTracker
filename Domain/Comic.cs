using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public enum ComicStatus
    {
        Unread = (short)0,
        Progress,
        Finished
    };

    public class Comic
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public ComicStatus Status { get; set; }
        public byte[]? Cover { get; set; }

        public int Order { get; set; }

        public virtual ICollection<Tag> Tags { get; set; } = [];
        public virtual ICollection<ComicFile> Files { get; set; } = [];


        public Comic() { Id = Guid.NewGuid(); }
    }
}
