using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain;

public class ComicFileData
{
    public Guid Id { get; set; }
    public Guid ComicFileId { get; set; }
    public byte[] Data { get; set; } = [];
    public virtual ComicFile File { get; set; } = new();

    public ComicFileData() { Id = Guid.NewGuid(); }
}
