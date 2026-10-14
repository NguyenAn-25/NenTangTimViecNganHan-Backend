using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetSkeleton.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSomethingElse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Examples");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Examples",
                type: "text",
                nullable: true);
        }
    }
}
