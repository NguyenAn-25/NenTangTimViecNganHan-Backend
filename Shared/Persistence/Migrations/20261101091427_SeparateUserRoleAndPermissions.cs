using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetSkeleton.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeparateUserRoleAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "user",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "USER");

            migrationBuilder.Sql("""
                UPDATE "user" AS app_user
                SET role = 'ADMIN'
                FROM permissions AS permission
                WHERE app_user.permission_id = permission.id
                  AND permission.name = 'ADMIN';

                UPDATE "user"
                SET role = 'USER'
                WHERE role <> 'ADMIN';

                UPDATE "user"
                SET permission_id = NULL
                WHERE role = 'USER';

                DELETE FROM permissions
                WHERE name = 'USER';

                UPDATE permissions
                SET name = 'ADMIN_DEFAULT'
                WHERE name = 'ADMIN';

                ALTER TABLE "user" ALTER COLUMN role DROP DEFAULT;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_user_role_permission",
                table: "user",
                sql: "(role = 'USER' AND permission_id IS NULL) OR (role = 'ADMIN' AND permission_id IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_user_role_permission",
                table: "user");

            migrationBuilder.Sql("""
                INSERT INTO permissions (status, name, description)
                VALUES ('ACTIVE', 'USER', 'Tài khoản người dùng')
                ON CONFLICT (name) DO NOTHING;

                UPDATE permissions
                SET name = 'ADMIN'
                WHERE name = 'ADMIN_DEFAULT';

                UPDATE "user"
                SET permission_id = (SELECT id FROM permissions WHERE name = 'USER')
                WHERE role = 'USER';
                """);

            migrationBuilder.DropColumn(
                name: "role",
                table: "user");
        }
    }
}
