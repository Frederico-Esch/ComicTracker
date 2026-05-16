using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public interface ITagRepository
    {
        public List<Tag> GetAll();
        public List<Tag> Exclude(HashSet<Guid> comicId, string? filter = null);
        public void Add(Tag tag);
        public void Remove(Tag tag);
    }
}
