using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    internal class ComicRepository(DataContext context) : IComicRepository
    {
        public void Add(Comic comic)
        {
            comic.Order = 1;
            foreach (var c in context.Comics.Where(c => c.Order >= 1))
            {
                c.Order += 1;
            }
            context.Comics.Add(comic);
        }

        public List<Comic> GetAllComics()
        {
            return context.Comics
                .Include(c => c.Tags)
                .OrderBy(c => c.Order)
                .ToList();
        }

        public Comic? FindOne(Guid id)
        {
            return context.Comics
                .Include(c => c.Tags)
                .FirstOrDefault(c => c.Id == id);
        }

        public void Delete(Comic comic)
        {
            context.Comics.Remove(comic);
        }
    }
}
