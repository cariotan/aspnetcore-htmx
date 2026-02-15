using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP.NET_Core_HTMX.Migrations.ErrorDatabase
{
    /// <inheritdoc />
    public partial class AddedHandledToUnhandledError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Handled",
                table: "UnhandledErrors",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Handled",
                table: "UnhandledErrors");
        }
    }
}
