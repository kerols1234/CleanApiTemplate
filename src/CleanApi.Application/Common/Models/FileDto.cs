namespace CleanApi.Application.Common.Models;

/// <summary>An in-memory file produced by the app (PDF/Excel export) for download by the API.</summary>
public sealed record FileDto(byte[] Content, string FileName, string ContentType);
