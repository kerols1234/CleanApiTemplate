namespace CleanApi.Domain.Common;

/// <summary>
/// Marker for a keyless type mapped to a database VIEW. Decorate the type with
/// <see cref="DbViewAttribute"/>; the Infrastructure model builder maps it with
/// <c>HasNoKey().ToView(name)</c> automatically via reflection.
/// </summary>
public interface IReadOnlyView;

/// <summary>
/// Marker for a keyless type returned by a STORED PROCEDURE or raw SQL query.
/// Mapped with <c>HasNoKey().ToView(null)</c> so it is query-only (never a table).
/// </summary>
public interface IReadOnlyStoredProc;

/// <summary>Names the database view a <see cref="IReadOnlyView"/> type maps to.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DbViewAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
