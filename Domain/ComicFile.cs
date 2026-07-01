using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain;

public class ComicFile
{
    public enum ComicFileType
    {
        CBZ,
        CBR
    }

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsFinished { get; set; }

    public int Order { get; set; }

    public Guid ComicId { get; set; }

    public ComicFileType FileType { get; set; }

    public virtual Comic Comic { get; set; }
    public virtual ComicFileData Data { get; set; }

#pragma warning disable CS8618 // O campo não anulável precisa conter um valor não nulo ao sair do construtor. Considere adicionar o modificador "obrigatório" ou declarar como anulável.
    public ComicFile() { Id = Guid.NewGuid(); }
#pragma warning restore CS8618 // O campo não anulável precisa conter um valor não nulo ao sair do construtor. Considere adicionar o modificador "obrigatório" ou declarar como anulável.
}
