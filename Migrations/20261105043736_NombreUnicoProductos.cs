using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibraUrbana.Migrations
{
    /// <inheritdoc />
    public partial class NombreUnicoProductos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT LOWER(LTRIM(RTRIM(Nombre)))
                    FROM Productos
                    GROUP BY LOWER(LTRIM(RTRIM(Nombre)))
                    HAVING COUNT(*) > 1
                )
                    THROW 51000, 'Existen nombres duplicados en Productos. Corrija los registros antes de aplicar NombreUnicoProductos.', 1;
                """);

            migrationBuilder.AddColumn<string>(
                name: "NombreNormalizado",
                table: "Productos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                computedColumnSql: "LOWER(LTRIM(RTRIM([Nombre])))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_NombreNormalizado",
                table: "Productos",
                column: "NombreNormalizado",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_NombreNormalizado",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "NombreNormalizado",
                table: "Productos");
        }
    }
}
