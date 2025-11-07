using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verifactu.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrosFacturacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Serie = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FechaHoraExpedicionUTC = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Huella = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    HuellaAnterior = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    EstadoEnvio = table.Column<int>(type: "INTEGER", nullable: false),
                    CodigoErrorAEAT = table.Column<int>(type: "INTEGER", nullable: true),
                    DescripcionErrorAEAT = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    XmlFirmado = table.Column<string>(type: "TEXT", nullable: false),
                    AcuseRecibo = table.Column<string>(type: "TEXT", nullable: true),
                    CSV = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FechaEnvio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaUltimoEnvio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Reintentos = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    NifEmisor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    NombreEmisor = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ImporteTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CuotaTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Anulado = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    FechaAnulacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RefExterna = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosFacturacion", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosFacturacion_EstadoEnvio",
                table: "RegistrosFacturacion",
                column: "EstadoEnvio");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosFacturacion_FechaHoraExpedicionUTC",
                table: "RegistrosFacturacion",
                column: "FechaHoraExpedicionUTC");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosFacturacion_Huella",
                table: "RegistrosFacturacion",
                column: "Huella");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosFacturacion_NifEmisor",
                table: "RegistrosFacturacion",
                column: "NifEmisor");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosFacturacion_Serie_Numero",
                table: "RegistrosFacturacion",
                columns: new[] { "Serie", "Numero" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosFacturacion");
        }
    }
}
