using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookwormsOnlineSecurity.Migrations
{
    public partial class AddUserLastSessionId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastSessionId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSessionId",
                table: "AspNetUsers");
        }
    }
}
