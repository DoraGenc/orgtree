using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrgTree.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddUnassignedDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO "Departments" ("Id", "Name")
                VALUES (-1, 'Keine Abteilung zugeordnet');

                UPDATE "Employees"
                SET "DepartmentId" = -1
                WHERE "DepartmentId" IS NULL
                   OR NOT EXISTS (
                       SELECT 1
                       FROM "Departments"
                       WHERE "Departments"."Id" = "Employees"."DepartmentId"
                   );
                """);

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Employees",
                type: "INTEGER",
                nullable: false,
                defaultValue: -1,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees");

            migrationBuilder.Sql(
                """
                UPDATE "Employees"
                SET "DepartmentId" = NULL
                WHERE "DepartmentId" = -1;
                """);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Employees",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: -1);
        }
    }
}
