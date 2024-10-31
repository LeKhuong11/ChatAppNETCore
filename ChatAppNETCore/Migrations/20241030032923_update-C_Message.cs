using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatAppNETCore.Migrations
{
    /// <inheritdoc />
    public partial class updateC_Message : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceiverId",
                table: "C_Messages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "isRead",
                table: "C_Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverId",
                table: "C_Messages");

            migrationBuilder.DropColumn(
                name: "isRead",
                table: "C_Messages");
        }
    }
}
