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
        public enum FilterType { Any, All }

        public void Add(Comic comic);
        public void Delete(Comic comic);
        public List<Comic> GetAllComics();
        public List<Comic> GetFiltered(List<Tag> tags, FilterType filter);
        public Comic? FindOne(Guid id);

        public void AddFile(byte[] file, string name, Comic comic);
        public void DeleteFile(ComicFile file);
        public Task<byte[]> LoadComicDataAsync(ComicFile comicFile);
    }
}
