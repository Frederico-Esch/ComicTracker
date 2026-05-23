using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories;

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

    private IQueryable<Comic> QueryComic() =>
        context.Comics.Include(c => c.Tags).Include(c => c.Files);

    public List<Comic> GetAllComics()
    {
        return QueryComic()
            .OrderBy(c => c.Order)
            .ToList();
    }

    public List<Comic> GetFiltered(List<Tag> tags, IComicRepository.FilterType filter)
    {
        var tagIds = tags.Select(t => t.Id).ToHashSet();

        switch (filter)
        {
            default:
            case IComicRepository.FilterType.Any:
                return QueryComic()
                    .OrderBy(c => c.Order)
                    .Where(c => c.Tags.Any(t => tagIds.Contains(t.Id)))
                    .ToList();
            case IComicRepository.FilterType.All:
                return QueryComic()
                    .OrderBy(c => c.Order)
                    .Where(c => tagIds.All(id => c.Tags.Any(t => t.Id == id)))
                    .ToList();
        }
    }

    public Comic? FindOne(Guid id)
    {
        return QueryComic()
            .FirstOrDefault(c => c.Id == id);
    }

    public void AddFile(byte[] file, string name, Comic comic)
    {
        var data = new ComicFileData()
        {
            Data = file,
        };
        var comicFile = new ComicFile()
        {
            Name = name,
            ComicId = comic.Id,
            Comic = comic,
            IsFinished = false,
            Order = comic.Files.Count > 0 ? comic.Files.Max(f => f.Order) + 1 : 1,
            Data = data
        };
        data.ComicFileId = comicFile.Id;

        context.ComicFiles.Add(comicFile);
        context.ComicFileData.Add(data);
        //comic.Files.Add(comicFile);
    }

    public void DeleteFile(ComicFile file)
    {
        context.ComicFiles.Remove(file);
    }

    public async Task<byte[]> LoadComicDataAsync(ComicFile comicFile)
    {
        var fileData = (await context.ComicFiles
            .Include(f => f.Data)
            .FirstOrDefaultAsync(f => f.Id ==  comicFile.Id))
            ?.Data
            ?.Data;
        fileData ??= [];
        return fileData;
    }

    public void Delete(Comic comic)
    {
        context.Comics.Remove(comic);
    }
}
