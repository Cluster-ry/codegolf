using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeGolf.Migrations
{
    /// <inheritdoc />
    public partial class AddFresherToggle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFresher",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFresher",
                table: "Groups");
        }
    }
}
