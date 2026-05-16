using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public interface IComicRepository
    {
        public void Add(Comic comic);
        public void Delete(Comic comic);
        public List<Comic> GetAllComics();
        public List<Comic> GetFiltered(List<Tag> tags);
        public Comic? FindOne(Guid id);
    }
}
