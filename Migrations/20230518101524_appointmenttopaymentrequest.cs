using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace latest.Migrations
{
    /// <inheritdoc />
    public partial class appointmenttopaymentrequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_PaymentRequests_PaymentRequestId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PaymentRequestId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PaymentRequestId",
                table: "Appointments");

            migrationBuilder.AddColumn<int>(
                name: "AppointmentId",
                table: "PaymentRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPayed",
                table: "PaymentRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_AppointmentId",
                table: "PaymentRequests",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequests_Appointments_AppointmentId",
                table: "PaymentRequests",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequests_Appointments_AppointmentId",
                table: "PaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_AppointmentId",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "IsPayed",
                table: "PaymentRequests");

            migrationBuilder.AddColumn<string>(
                name: "PaymentRequestId",
                table: "Appointments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PaymentRequestId",
                table: "Appointments",
                column: "PaymentRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_PaymentRequests_PaymentRequestId",
                table: "Appointments",
                column: "PaymentRequestId",
                principalTable: "PaymentRequests",
                principalColumn: "Id");
        }
    }
}
