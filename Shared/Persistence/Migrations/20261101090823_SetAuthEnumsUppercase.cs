using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetSkeleton.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SetAuthEnumsUppercase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE auth_otps
                SET purpose = UPPER(purpose);

                UPDATE permissions
                SET name = UPPER(name), status = UPPER(status);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE auth_otps
                SET purpose = LOWER(purpose);

                UPDATE permissions
                SET name = LOWER(name), status = LOWER(status);
                """);
        }
    }
}
