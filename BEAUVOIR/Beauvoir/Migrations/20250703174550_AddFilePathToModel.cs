using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beauvoir.Migrations
{
    /// <inheritdoc />
    public partial class AddFilePathToModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "Model");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Model",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Model");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileContent",
                table: "Model",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
