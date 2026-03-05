using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarismaKiosk.Migrations.PersistantDataDatabase
{
    /// <inheritdoc />
    public partial class AddedDateCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "PersistantData",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "PersistantData");
        }
    }
}
