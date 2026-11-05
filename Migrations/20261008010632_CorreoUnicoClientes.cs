using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibraUrbana.Migrations
{
    /// <inheritdoc />
    public partial class CorreoUnicoClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT LOWER(LTRIM(RTRIM(Correo)))
                    FROM Clientes
                    WHERE Correo IS NOT NULL AND Correo <> ''
                    GROUP BY LOWER(LTRIM(RTRIM(Correo)))
                    HAVING COUNT(*) > 1
                )
                    THROW 51000, 'Existen correos duplicados en Clientes. Corrija los registros antes de aplicar CorreoUnicoClientes.', 1;
                """);

            migrationBuilder.AddColumn<string>(
                name: "CorreoNormalizado",
                table: "Clientes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                computedColumnSql: "NULLIF(LOWER(LTRIM(RTRIM([Correo]))), '')",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_CorreoNormalizado",
                table: "Clientes",
                column: "CorreoNormalizado",
                unique: true,
                filter: "[Correo] IS NOT NULL AND [Correo] <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_CorreoNormalizado",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CorreoNormalizado",
                table: "Clientes");
        }
    }
}
