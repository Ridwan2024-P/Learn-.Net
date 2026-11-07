using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SignalRMvcDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddDeleteForSenderAndReceiver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeletedForReceiver",
                table: "ChatMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletedForSender",
                table: "ChatMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeletedForReceiver",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "IsDeletedForSender",
                table: "ChatMessages");
        }
    }
}
