namespace CleanApi.Application.Common.Extensions;

/// <summary>General-purpose <see cref="IEnumerable{T}"/> helpers.</summary>
public static class EnumerableExtensions
{
    /// <summary>True when the sequence is null or contains no elements.</summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) => source is null || !source.Any();

    /// <summary>
    /// Reconciles two sets by a shared key, returning what to add, update, and remove — the classic
    /// pattern for syncing a child collection against an incoming edit request.
    /// </summary>
    /// <returns>
    /// <c>ToAdd</c>: items in <paramref name="incoming"/> with no match in <paramref name="existing"/>.
    /// <c>ToUpdate</c>: matched (incoming, existing) pairs. <c>ToRemove</c>: existing items no longer present.
    /// </returns>
    public static (List<TIncoming> ToAdd, List<(TIncoming Incoming, TExisting Existing)> ToUpdate, List<TExisting> ToRemove)
        Reconcile<TIncoming, TExisting, TKey>(
            this IEnumerable<TIncoming> incoming,
            IEnumerable<TExisting> existing,
            Func<TIncoming, TKey> incomingKey,
            Func<TExisting, TKey> existingKey)
        where TKey : notnull
    {
        var existingByKey = existing.ToDictionary(existingKey);
        var incomingKeys = new HashSet<TKey>();

        var toAdd = new List<TIncoming>();
        var toUpdate = new List<(TIncoming, TExisting)>();

        foreach (var item in incoming)
        {
            var key = incomingKey(item);
            incomingKeys.Add(key);

            if (existingByKey.TryGetValue(key, out var match))
            {
                toUpdate.Add((item, match));
            }
            else
            {
                toAdd.Add(item);
            }
        }

        var toRemove = existingByKey
            .Where(kvp => !incomingKeys.Contains(kvp.Key))
            .Select(kvp => kvp.Value)
            .ToList();

        return (toAdd, toUpdate, toRemove);
    }
}
