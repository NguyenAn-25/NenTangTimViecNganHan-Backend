using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetSkeleton.Shared.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserVerificationState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Some existing databases already have is_verified even though the migration
            // history does not record its creation. Keep startup migration safe for both shapes.
            migrationBuilder.Sql(
                """
                ALTER TABLE "user"
                    ADD COLUMN IF NOT EXISTS is_verified boolean NOT NULL DEFAULT FALSE;
                ALTER TABLE "user"
                    ADD COLUMN IF NOT EXISTS verified_at timestamp with time zone NULL;
                ALTER TABLE "user"
                    ALTER COLUMN verified_at DROP NOT NULL;
                """);

            // Existing active accounts and rows already marked verified keep that state. For
            // inactive rows, use the latest registration OTP when it was verified.
            migrationBuilder.Sql(
                """
                UPDATE "user" AS u
                SET is_verified = (
                        COALESCE(u.is_verified, FALSE)
                        OR u.is_active
                        OR u.verified_at IS NOT NULL
                        OR (
                            SELECT otp.verified_at
                            FROM auth_otps AS otp
                            WHERE otp.user_id = u.id
                              AND otp.purpose = 'REGISTRATION'
                            ORDER BY otp.created_at DESC, otp.id DESC
                            LIMIT 1
                        ) IS NOT NULL
                    ),
                    verified_at = CASE
                        WHEN COALESCE(u.is_verified, FALSE)
                          OR u.is_active
                          OR u.verified_at IS NOT NULL
                          OR (
                                SELECT otp.verified_at
                                FROM auth_otps AS otp
                                WHERE otp.user_id = u.id
                                  AND otp.purpose = 'REGISTRATION'
                                ORDER BY otp.created_at DESC, otp.id DESC
                                LIMIT 1
                             ) IS NOT NULL
                        THEN COALESCE(
                            u.verified_at,
                            (
                                SELECT otp.verified_at
                                FROM auth_otps AS otp
                                WHERE otp.user_id = u.id
                                  AND otp.purpose = 'REGISTRATION'
                                ORDER BY otp.created_at DESC, otp.id DESC
                                LIMIT 1
                            ),
                            u.created_at)
                        ELSE NULL
                    END;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE "user"
                    ALTER COLUMN is_verified SET DEFAULT FALSE;
                ALTER TABLE "user"
                    ALTER COLUMN is_verified SET NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint AS existing_constraint
                        WHERE existing_constraint.conname = 'CK_user_verification_state'
                          AND existing_constraint.conrelid = to_regclass('"user"')
                    ) THEN
                        ALTER TABLE "user"
                            ADD CONSTRAINT "CK_user_verification_state"
                            CHECK ((is_verified = TRUE AND verified_at IS NOT NULL)
                                OR (is_verified = FALSE AND verified_at IS NULL));
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // is_verified is retained because it may predate this migration in an existing DB,
            // and it remains part of the user verification state.
            migrationBuilder.Sql(
                """
                ALTER TABLE "user"
                    DROP CONSTRAINT IF EXISTS "CK_user_verification_state";
                ALTER TABLE "user"
                    DROP COLUMN IF EXISTS verified_at;
                """);
        }
    }
}
