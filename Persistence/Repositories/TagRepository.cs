using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    internal class TagRepository(DataContext context) : ITagRepository
    {
        public void Add(Tag tag)
        {
            context.Tags.Add(tag);
        }

        public List<Tag> GetAll()
        {
            return context.Tags.ToList();
        }

        public void Remove(Tag tag)
        {
            context.Tags.Remove(tag);
        }

        public List<Tag> Exclude(HashSet<Guid> excludedTags, string? filter = null)
        {
            var query = context.Tags.AsEnumerable();

            if (filter != null)
                query = query
                    .Where(t => t.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase));

            return query.Where(t => !excludedTags.Contains(t.Id)).ToList();
        }
    }
}
