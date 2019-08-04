﻿// <auto-generated />
using System;
using ITLab.Projects.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ITLab.Projects.Database.Migrations
{
    [DbContext(typeof(ProjectsContext))]
    partial class ProjectsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "3.0.0-preview7.19362.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("ITLab.Projects.Models.Participation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("From");

                    b.Property<Guid>("ProjectId");

                    b.Property<Guid>("ProjectRoleId");

                    b.Property<DateTime>("To");

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.HasIndex("ProjectRoleId");

                    b.ToTable("Participations");
                });

            modelBuilder.Entity("ITLab.Projects.Models.Project", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("Description");

                    b.Property<string>("GitRepoLink");

                    b.Property<string>("LogoLink");

                    b.Property<string>("Name");

                    b.Property<string>("TasksLink");

                    b.HasKey("Id");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("ITLab.Projects.Models.ProjectRole", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("ProjectRoles");
                });

            modelBuilder.Entity("ITLab.Projects.Models.ProjectTag", b =>
                {
                    b.Property<Guid>("ProjectId");

                    b.Property<Guid>("TagId");

                    b.HasKey("ProjectId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("ProjectTags");
                });

            modelBuilder.Entity("ITLab.Projects.Models.Tag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("ITLab.Projects.Models.Participation", b =>
                {
                    b.HasOne("ITLab.Projects.Models.Project", "Project")
                        .WithMany("Participations")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ITLab.Projects.Models.ProjectRole", "ProjectRole")
                        .WithMany("Participations")
                        .HasForeignKey("ProjectRoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ITLab.Projects.Models.ProjectTag", b =>
                {
                    b.HasOne("ITLab.Projects.Models.Project", "Project")
                        .WithMany("ProjectTags")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ITLab.Projects.Models.Tag", "Tag")
                        .WithMany("ProjectTags")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
