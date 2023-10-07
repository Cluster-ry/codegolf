using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeGolf.Migrations
{
    /// <inheritdoc />
    public partial class ExpandProblemData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExampleSolution",
                table: "Problems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Explanation",
                table: "Problems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Answers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExampleSolution",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "Explanation",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Answers");
        }
    }
}
