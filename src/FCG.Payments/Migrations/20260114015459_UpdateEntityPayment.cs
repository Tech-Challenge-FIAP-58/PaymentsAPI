using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCG.Payments.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntityPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentType",
                table: "Payments",
                newName: "PaymentMethod");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "Payments",
                newName: "PaymentType");
        }
    }
}
