using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BuildingBlocks.Extensions.Types;

/// <summary>
/// A custom trace listener that filters out trace messages based on specified namespaces.
/// </summary>
/// <param name="namespacesToFilter">An enumerable of namespaces to filter out.</param>
public class FilterTraceListener(IEnumerable<string> namespacesToFilter) : DefaultTraceListener
{
    /// <summary>
    /// A regular expression used to match the namespaces to filter out.
    /// </summary>
    private readonly Regex _regex = new($"^({string.Join("|", namespacesToFilter.Select(Regex.Escape))}):");

    /// <summary>
    /// Writes a message to the listener, but only if it does not match the filtered namespaces.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public override void WriteLine(string? message)
    {
        if (_regex.IsMatch(message ?? string.Empty)) return;
        base.WriteLine(message);
    }         

    /// <summary>
    /// Writes a message to the listener, but only if it does not match the filtered namespaces.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public override void Write(string? message)
    {
        if (_regex.IsMatch(message ?? string.Empty)) return;
        base.Write(message);
    }
}

/// <summary>
/// Provides extension methods for the <see cref="Trace"/> class.
/// </summary>
public static class TraceExtensions
{
    /// <summary>
    /// Clears the existing trace listeners and adds a new <see cref="FilterTraceListener"/> 
    /// that filters out trace messages based on the specified namespaces.
    /// </summary>
    /// <param name="namespacesToFilter">An array of namespaces to filter out. 
    /// Messages from these namespaces will not be written to the trace listeners.</param>
    public static void Filter(params string[] namespacesToFilter)
    {
        Trace.Listeners.Clear();
        Trace.Listeners.Add(new FilterTraceListener(namespacesToFilter));
    }
}