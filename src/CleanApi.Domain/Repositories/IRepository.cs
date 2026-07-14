using Ardalis.Specification;

namespace CleanApi.Domain.Repositories;

/// <summary>
/// Read/write repository over an aggregate root, driven by the specification pattern
/// (Ardalis.Specification). Queries are expressed as <c>Specification&lt;T&gt;</c> objects so
/// query logic stays testable and out of the handlers.
/// </summary>
public interface IRepository<T> : IRepositoryBase<T>
    where T : class;

/// <summary>Read-only counterpart of <see cref="IRepository{T}"/> for query handlers.</summary>
public interface IReadRepository<T> : IReadRepositoryBase<T>
    where T : class;
