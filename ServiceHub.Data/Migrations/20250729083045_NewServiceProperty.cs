using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewServiceProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBusinessOnly",
                table: "Services");

            migrationBuilder.AddColumn<int>(
                name: "ViewsCount",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewsCount",
                table: "Services");

            migrationBuilder.AddColumn<bool>(
                name: "IsBusinessOnly",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
