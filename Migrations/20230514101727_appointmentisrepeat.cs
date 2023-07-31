using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace latest.Migrations
{
    /// <inheritdoc />
    public partial class appointmentisrepeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRepeat",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRepeat",
                table: "Appointments");
        }
    }
}
