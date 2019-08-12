using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ITLab.Projects.Database.Migrations
{
    public partial class TagColor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Tags",
                nullable: false,
                defaultValue: "#ffffff")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Tags");
        }
    }
}
