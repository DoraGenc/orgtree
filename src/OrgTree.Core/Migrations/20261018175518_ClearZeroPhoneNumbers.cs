using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrgTree.Core.Migrations
{
    /// <inheritdoc />
    public partial class ClearZeroPhoneNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Employees SET PhoneNumber = NULL WHERE PhoneNumber = '0'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
