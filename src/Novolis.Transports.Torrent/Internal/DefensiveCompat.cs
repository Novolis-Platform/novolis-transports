using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DefensiveProgrammingFramework;

/// <summary>
///     Minimal net10 stand-in for the legacy DefensiveProgrammingFramework NuGet (x86 / .NET Framework).
/// </summary>
public static class GuardExtensions
{
    public static void CannotBeNull([NotNull] this object? value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value is null)
            throw new ArgumentNullException(name);
    }

    public static void CannotBeNullOrEmpty([NotNull] this string? value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty.", name);
    }

    public static void CannotBeNullOrEmpty([NotNull] this IEnumerable? value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value is null)
            throw new ArgumentNullException(name);
        if (!value.Cast<object?>().Any())
            throw new ArgumentException("Sequence cannot be empty.", name);
    }

    public static void CannotBeNullOrEmpty([NotNull] this byte[]? value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value is null || value.Length == 0)
            throw new ArgumentException("Buffer cannot be null or empty.", name);
    }

    public static void CannotContainOnlyNull(this IEnumerable? value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        value.CannotBeNull(name);
        // Original API rejects sequences that are null-only; empty is allowed here.
        if (value!.Cast<object?>().Any() && value.Cast<object?>().All(x => x is null))
            throw new ArgumentException("Sequence cannot contain only null values.", name);
    }

    public static void MustBeGreaterThan(this int value, int min, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value <= min)
            throw new ArgumentOutOfRangeException(name, value, $"Value must be greater than {min}.");
    }

    public static void MustBeGreaterThan(this long value, long min, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value <= min)
            throw new ArgumentOutOfRangeException(name, value, $"Value must be greater than {min}.");
    }

    public static void MustBeGreaterThan(this decimal value, decimal min, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value <= min)
            throw new ArgumentOutOfRangeException(name, value, $"Value must be greater than {min}.");
    }

    public static void MustBeGreaterThan(this TimeSpan value, TimeSpan min, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value <= min)
            throw new ArgumentOutOfRangeException(name, value, $"Value must be greater than {min}.");
    }

    public static void MustBeGreaterThanOrEqualTo(this int value, int min, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value < min)
            throw new ArgumentOutOfRangeException(name, value, $"Value must be >= {min}.");
    }

    public static void MustBeGreaterThanOrEqualTo(this long value, long min, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value < min)
            throw new ArgumentOutOfRangeException(name, value, $"Value must be >= {min}.");
    }

    public static void MustBeGreaterThanOrEqualTo(this decimal value, decimal min, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value < min)
            throw new ArgumentOutOfRangeException(name, value, $"Value must be >= {min}.");
    }

    public static void MustBeGreaterThanOrEqualTo(this TimeSpan value, TimeSpan min, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value < min)
            throw new ArgumentOutOfRangeException(name, value, $"Value must be >= {min}.");
    }

    public static void MustBeLessThan(this int value, int max, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value >= max)
            throw new ArgumentOutOfRangeException(name, value, $"Value must be less than {max}.");
    }

    public static void MustBeLessThanOrEqualTo(this int value, int max, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value > max)
            throw new ArgumentOutOfRangeException(name, value, $"Value must be <= {max}.");
    }

    public static void MustBeLessThanOrEqualTo(this long value, long max, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value > max)
            throw new ArgumentOutOfRangeException(name, value, $"Value must be <= {max}.");
    }

    public static void MustBeLessThan(this long value, long max, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value >= max)
            throw new ArgumentOutOfRangeException(name, value, $"Value must be less than {max}.");
    }

    public static void MustBe(this string value, Func<string, bool> predicate, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        value.CannotBeNull(name);
        if (!predicate(value))
            throw new ArgumentException("Value failed predicate.", name);
    }

    public static void MustFileExist(this string value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        value.CannotBeNullOrEmpty(name);
        if (!File.Exists(value))
            throw new FileNotFoundException("File does not exist.", value);
    }

    public static void MustBeEqualTo(this int value, int expected, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value != expected)
            throw new ArgumentOutOfRangeException(name, value, $"Value must equal {expected}.");
    }

    public static void MustBeEqualTo(this long value, long expected, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value != expected)
            throw new ArgumentOutOfRangeException(name, value, $"Value must equal {expected}.");
    }

    public static void CannotBeEqualTo(this TimeSpan value, TimeSpan forbidden, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value == forbidden)
            throw new ArgumentOutOfRangeException(name, value, $"Value cannot equal {forbidden}.");
    }

    public static void MustBeOneOf(this int value, params int[] allowed)
    {
        if (allowed is null || !allowed.Contains(value))
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value is not in the allowed set.");
    }

    public static void MustBeValidDirectoryPath(this string value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        value.CannotBeNullOrEmpty(name);
        if (value.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            throw new ArgumentException("Path contains invalid characters.", name);
    }

    public static void MustBeValidFilePath(this string value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        value.CannotBeNullOrEmpty(name);
        if (value.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            throw new ArgumentException("Path contains invalid characters.", name);
    }

    public static bool IsNotNull([NotNullWhen(true)] this object? value) => value is not null;

    public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? value) => !string.IsNullOrEmpty(value);

    public static bool IsNotNullOrEmpty([NotNullWhen(true)] this byte[]? value) => value is { Length: > 0 };

    public static void Then(this bool condition, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (condition)
            action();
    }
}
