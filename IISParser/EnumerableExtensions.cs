using System;
using System.Collections.Generic;

namespace IISParser;

/// <summary>
/// Provides extension methods for lazy enumeration over <see cref="IEnumerable{T}"/>.
/// </summary>
public static class EnumerableExtensions {
    /// <summary>
    /// Lazily returns only the last <paramref name="count"/> elements of the source sequence.
    /// </summary>
    /// <typeparam name="T">Type of elements.</typeparam>
    /// <param name="source">Sequence to enumerate.</param>
    /// <param name="count">Number of elements to take from the end.</param>
    /// <returns>The last <paramref name="count"/> elements in order.</returns>
    public static IEnumerable<T> TakeLastLazy<T>(this IEnumerable<T> source, int count) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (count <= 0) {
            yield break;
        }

        var queue = new Queue<T>(count);
        foreach (var item in source) {
            if (queue.Count == count) {
                queue.Dequeue();
            }
            queue.Enqueue(item);
        }

        foreach (var item in queue) {
            yield return item;
        }
    }

    /// <summary>
    /// Lazily skips the final <paramref name="count"/> elements of the source sequence.
    /// </summary>
    /// <typeparam name="T">Type of elements.</typeparam>
    /// <param name="source">Sequence to enumerate.</param>
    /// <param name="count">Number of elements to omit from the end.</param>
    /// <returns>All elements except the last <paramref name="count"/>.</returns>
    public static IEnumerable<T> SkipLastLazy<T>(this IEnumerable<T> source, int count) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (count <= 0) {
            foreach (var item in source) {
                yield return item;
            }
            yield break;
        }

        var queue = new Queue<T>(count + 1);
        foreach (var item in source) {
            queue.Enqueue(item);
            if (queue.Count > count) {
                yield return queue.Dequeue();
            }
        }
    }
}

