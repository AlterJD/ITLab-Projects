using ITLab.Projects.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ITLab.Projects.Database
{
    public class ProjectsContext : DbContext
    {
        public DbSet<Project> Projects { get; set; }

        public ProjectsContext(DbContextOptions options) : base(options)
        {

        }
    }

    internal class ProjectsContextFactory : IDesignTimeDbContextFactory<ProjectsContext>
    {
        public ProjectsContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProjectsContext>();

            var config = (new ConfigurationBuilder())
                                        .AddJsonFile("appsettings.Secret.json", false)
                                        .Build();

            var connectionString = config.GetConnectionString("Postgres");
            optionsBuilder.UseNpgsql(connectionString);
            return new ProjectsContext(optionsBuilder.Options);
        }
    }
}
