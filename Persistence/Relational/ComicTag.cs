using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Relational
{
    internal class ComicTag
    {
        public Guid Id { get; set; }
        public Guid TagId { get; set; }
        public Guid ComicId { get; set; }

        public virtual Comic Comic { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}
