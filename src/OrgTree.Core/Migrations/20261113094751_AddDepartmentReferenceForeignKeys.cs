using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrgTree.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentReferenceForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Departments"
                SET "LeaderId" = NULL
                WHERE "LeaderId" IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "Employees"
                      WHERE "Employees"."Id" = "Departments"."LeaderId"
                  );

                UPDATE "Departments"
                SET "ParentDepartmentId" = NULL
                WHERE "ParentDepartmentId" IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "Departments" AS "Parent"
                      WHERE "Parent"."Id" = "Departments"."ParentDepartmentId"
                  );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_LeaderId",
                table: "Departments",
                column: "LeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_LeaderId",
                table: "Departments",
                column: "LeaderId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Departments_ParentDepartmentId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_LeaderId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_LeaderId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_ParentDepartmentId",
                table: "Departments");
        }
    }
}
