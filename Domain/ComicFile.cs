using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain;

public class ComicFile
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsFinished { get; set; }

    public int Order { get; set; }

    public Guid ComicId { get; set; }

    public virtual Comic Comic { get; set; }
    public virtual ComicFileData Data { get; set; }

    public ComicFile() { Id = Guid.NewGuid(); }
}
