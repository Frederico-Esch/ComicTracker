using Microsoft.Extensions.DependencyInjection;
using Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence
{
    public static class ServiceConfig
    {
        public static void ConfigRepositories(this IServiceCollection services)
        {
            services.AddDbContext<DataContext>();
            services.AddTransient<IComicRepository, ComicRepository>();
            services.AddTransient<ITagRepository, TagRepository>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
        }
    }
}
