using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatAppNETCore.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "C_Notification",
                newName: "ReceiveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReceiveId",
                table: "C_Notification",
                newName: "UserId");
        }
    }
}
