using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beauvoir.Migrations
{
    /// <inheritdoc />
    public partial class AddFileContentToModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Model",
                newName: "FileName");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileContent",
                table: "Model",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "FileExtension",
                table: "Model",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "Model");

            migrationBuilder.DropColumn(
                name: "FileExtension",
                table: "Model");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Model",
                newName: "FilePath");
        }
    }
}
