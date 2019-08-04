using ITLab.Projects.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ITLab.Projects.Database
{
    public class ProjectsContext : DbContext
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ProjectTag> ProjectTags { get; set; }
        public DbSet<ProjectRole> ProjectRoles { get; set; }
        public DbSet<Participation> Participations { get; set; }

        public ProjectsContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ProjectTag>(b =>
            {
                b.HasKey(pt => new { pt.ProjectId, pt.TagId });

                b.HasOne(pt => pt.Project).WithMany(p => p.ProjectTags).HasForeignKey(pt => pt.ProjectId);
                b.HasOne(pt => pt.Tag).WithMany(p => p.ProjectTags).HasForeignKey(pt => pt.TagId);
            });
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
