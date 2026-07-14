using Microsoft.EntityFrameworkCore.Migrations;

namespace CleanApi.Infrastructure.Persistence.Extensions;

/// <summary>Helpers for creating/dropping views and stored procedures inside migrations (idempotent).</summary>
public static class MigrationBuilderExtensions
{
    public static void CreateOrAlterView(this MigrationBuilder migrationBuilder, string viewName, string selectStatement)
    {
        migrationBuilder.Sql($"CREATE OR ALTER VIEW [{viewName}] AS {selectStatement}");
    }

    public static void DropView(this MigrationBuilder migrationBuilder, string viewName)
    {
        migrationBuilder.Sql($"DROP VIEW IF EXISTS [{viewName}]");
    }

    public static void CreateOrAlterStoredProcedure(this MigrationBuilder migrationBuilder, string procName, string body)
    {
        migrationBuilder.Sql($"CREATE OR ALTER PROCEDURE [{procName}] {body}");
    }

    public static void DropStoredProcedure(this MigrationBuilder migrationBuilder, string procName)
    {
        migrationBuilder.Sql($"DROP PROCEDURE IF EXISTS [{procName}]");
    }
}
