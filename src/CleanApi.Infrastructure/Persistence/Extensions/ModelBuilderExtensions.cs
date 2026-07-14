using System.Reflection;
using CleanApi.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CleanApi.Infrastructure.Persistence.Extensions;

/// <summary>
/// Reflection-based mapping for keyless database objects, so adding a new view/proc result type
/// requires no DbContext edit — just implement the marker interface (and, for views, the attribute).
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>Maps every <see cref="IReadOnlyView"/> in the assembly to its <see cref="DbViewAttribute"/> view.</summary>
    public static void ApplyDatabaseViews(this ModelBuilder modelBuilder, Assembly assembly)
    {
        foreach (var type in KeylessTypes<IReadOnlyView>(assembly))
        {
            var viewName = type.GetCustomAttribute<DbViewAttribute>()?.Name ?? type.Name;
            modelBuilder.Entity(type).HasNoKey().ToView(viewName);
        }
    }

    /// <summary>Registers every <see cref="IReadOnlyStoredProc"/> result type as a keyless, query-only type.</summary>
    public static void ApplyStoredProcedureResults(this ModelBuilder modelBuilder, Assembly assembly)
    {
        foreach (var type in KeylessTypes<IReadOnlyStoredProc>(assembly))
        {
            // ToView(null) => query-only: usable with FromSql, never treated as a table.
            modelBuilder.Entity(type).HasNoKey().ToView(null);
        }
    }

    private static IEnumerable<Type> KeylessTypes<TMarker>(Assembly assembly) =>
        assembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false } && typeof(TMarker).IsAssignableFrom(t));
}
