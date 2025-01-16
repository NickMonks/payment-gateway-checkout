using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentGateway.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:currency", "usd,eur,gbp")
                .Annotation("Npgsql:Enum:payment_status", "authorized,declined,rejected");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentStatus = table.Column<string>(type: "text", nullable: false),
                    CardNumberFourDigits = table.Column<int>(type: "integer", nullable: false),
                    ExpirationMonth = table.Column<string>(type: "text", nullable: false),
                    ExpirationYear = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentId",
                table: "Payments",
                column: "PaymentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}