using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRowVersionDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Habilitar extensão pgcrypto se não estiver habilitada
            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS pgcrypto;");

            // Para PostgreSQL, o rowVersion precisa de um trigger para atualizar automaticamente
            // Vamos criar um trigger que atualiza o RowVersion a cada UPDATE
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION update_row_version()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.""RowVersion"" = gen_random_bytes(8);
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                DROP TRIGGER IF EXISTS update_orders_row_version ON ""Orders"";
                CREATE TRIGGER update_orders_row_version
                    BEFORE UPDATE ON ""Orders""
                    FOR EACH ROW
                    EXECUTE FUNCTION update_row_version();
            ");

            // Atualizar registros existentes com um valor inicial
            migrationBuilder.Sql(@"
                UPDATE ""Orders""
                SET ""RowVersion"" = gen_random_bytes(8)
                WHERE ""RowVersion"" IS NULL OR ""RowVersion"" = '\\x'::bytea;
            ");

            // Adicionar valor padrão para novos registros
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
                DROP TRIGGER IF EXISTS update_orders_row_version ON ""Orders"";
                DROP FUNCTION IF EXISTS update_row_version();
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
