using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Configure.Models.Configure.Interfaces;

namespace ITLab.Projects.Database
{
    public class MigrationApplyWork : IConfigureWork
    {
        private readonly ProjectsContext context;
        private readonly ILogger<MigrationApplyWork> logger;

        private int tryCount = 10;
        private TimeSpan tryPeriod = TimeSpan.FromSeconds(10);

        public MigrationApplyWork(
            ProjectsContext context,
            ILogger<MigrationApplyWork> logger)
        {
            this.context = context;
            this.logger = logger;
        }
        public async Task Configure()
        {
            try
            {
                var pending = await context.Database.GetPendingMigrationsAsync();
                if (pending.Any())
                    await context.Database.MigrateAsync();
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Error while apply migrations");
            }
            catch (Exception ex)
            {
                if (tryCount == 0)
                    throw;
                logger.LogWarning(ex, "Error while apply migrations");
                tryCount--;
                await Task.Delay(tryPeriod);
                await Configure();
            }
        }
    }
}
