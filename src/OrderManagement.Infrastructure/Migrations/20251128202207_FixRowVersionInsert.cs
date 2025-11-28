using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRowVersionInsert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Habilitar extensão pgcrypto se não estiver habilitada
            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS pgcrypto;");

            // Criar trigger para INSERT que gera RowVersion se for NULL ou vazio
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION set_row_version_on_insert()
                RETURNS TRIGGER AS $$
                BEGIN
                    IF NEW.""RowVersion"" IS NULL OR NEW.""RowVersion"" = '\\x'::bytea THEN
                        NEW.""RowVersion"" = gen_random_bytes(8);
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                DROP TRIGGER IF EXISTS set_orders_row_version_on_insert ON ""Orders"";
                CREATE TRIGGER set_orders_row_version_on_insert
                    BEFORE INSERT ON ""Orders""
                    FOR EACH ROW
                    EXECUTE FUNCTION set_row_version_on_insert();
            ");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Orders",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "gen_random_bytes(8)",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS set_orders_row_version_on_insert ON ""Orders"";
                DROP FUNCTION IF EXISTS set_row_version_on_insert();
            ");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Orders",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldDefaultValueSql: "gen_random_bytes(8)");
        }
    }
}
