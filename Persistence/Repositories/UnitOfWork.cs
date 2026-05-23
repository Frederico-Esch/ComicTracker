
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    internal class UnitOfWork(DataContext context) : IUnitOfWork
    {
        private Task? saving;
        public void DiscardChanges()
        {
            foreach (var entity in context.ChangeTracker.Entries())
            {
                switch (entity.State)
                {
                    case EntityState.Modified:
                        entity.State = EntityState.Unchanged;
                        break;
                    case EntityState.Deleted:
                        entity.State = EntityState.Modified;
                        entity.State = EntityState.Unchanged;
                        break;
                    case EntityState.Unchanged:
                        break;
                }
            }
        }

        public bool HasChanges() => context.ChangeTracker.HasChanges();

        public async Task SaveAsync() => await context.SaveChangesAsync();

        public void Save() => context.SaveChanges();

        public void ScheduleSave()
        {
            if (saving is not null)
            {
                if (!saving.IsCompleted)
                    saving.GetAwaiter().GetResult();
            }
            saving = Task.Run(async () => { await context.SaveChangesAsync(); });
        }
    }
}
