using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrgTree.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmployeeDepartmentHead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TEMP TABLE DepartmentHeadMigrationGuard (DepartmentId INTEGER NOT NULL);
                CREATE TEMP TRIGGER DepartmentHeadMigrationGuardTrigger
                BEFORE INSERT ON DepartmentHeadMigrationGuard
                WHEN (
                    SELECT COUNT(*)
                    FROM Employees
                    WHERE Employees.DepartmentId = NEW.DepartmentId
                      AND Employees.DepartmentHead = 1
                ) > 1
                BEGIN
                    SELECT RAISE(ABORT, 'Multiple department heads found; resolve them before removing DepartmentHead.');
                END;
                INSERT INTO DepartmentHeadMigrationGuard (DepartmentId)
                SELECT Id FROM Departments;
                DROP TRIGGER DepartmentHeadMigrationGuardTrigger;
                DROP TABLE DepartmentHeadMigrationGuard;
                """);

            migrationBuilder.Sql(
                """
                UPDATE Departments
                SET LeaderId = (
                    SELECT Employees.Id
                    FROM Employees
                    WHERE Employees.DepartmentId = Departments.Id
                      AND Employees.DepartmentHead = 1
                )
                WHERE LeaderId IS NULL
                  AND (
                    SELECT COUNT(*)
                    FROM Employees
                    WHERE Employees.DepartmentId = Departments.Id
                      AND Employees.DepartmentHead = 1
                  ) = 1;
                """);

            migrationBuilder.DropColumn(
                name: "DepartmentHead",
                table: "Employees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DepartmentHead",
                table: "Employees",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE Employees
                SET DepartmentHead = CASE
                    WHEN DepartmentId IS NULL THEN NULL
                    WHEN EXISTS (
                        SELECT 1
                        FROM Departments
                        WHERE Departments.Id = Employees.DepartmentId
                          AND Departments.LeaderId = Employees.Id
                    ) THEN 1
                    ELSE 0
                END;
                """);
        }
    }
}
