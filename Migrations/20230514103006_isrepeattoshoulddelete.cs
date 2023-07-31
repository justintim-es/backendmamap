using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace latest.Migrations
{
    /// <inheritdoc />
    public partial class isrepeattoshoulddelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsRepeat",
                table: "Appointments",
                newName: "ShouldDelete");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShouldDelete",
                table: "Appointments",
                newName: "IsRepeat");
        }
    }
}
