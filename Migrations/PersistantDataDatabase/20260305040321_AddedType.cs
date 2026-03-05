using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarismaKiosk.Migrations.PersistantDataDatabase
{
    /// <inheritdoc />
    public partial class AddedType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "PersistantData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "PersistantData");
        }
    }
}
