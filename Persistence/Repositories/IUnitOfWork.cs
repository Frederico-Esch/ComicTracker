using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public interface IUnitOfWork
    {
        public void Save();
        public void ScheduleSave();
        public bool HasChanges();
        public void DiscardChanges();
    }
}
