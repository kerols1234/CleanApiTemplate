using Ardalis.Specification.EntityFrameworkCore;
using CleanApi.Domain.Repositories;

namespace CleanApi.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the specification-based repository (Ardalis.Specification). One generic
/// type serves every aggregate; query logic lives in <c>Specification&lt;T&gt;</c> classes, not here.
/// </summary>
public sealed class EfRepository<T>(AppDbContext dbContext)
    : RepositoryBase<T>(dbContext), IRepository<T>, IReadRepository<T>
    where T : class;
