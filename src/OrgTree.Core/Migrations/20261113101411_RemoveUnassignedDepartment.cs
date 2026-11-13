using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrgTree.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnassignedDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: -1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "ColorHex", "LeaderId", "Name", "ParentDepartmentId" },
                values: new object[] { -1, null, null, "Keine Abteilung zugeordnet", null });

            migrationBuilder.Sql(
                """
                UPDATE "Employees"
                SET "DepartmentId" = -1
                WHERE "DepartmentId" IS NULL;
                """);
        }
    }
}
