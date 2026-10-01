using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace DigisimPlugin.TestHelpers;

/// <summary>Records every message at or above <see cref="MinLevel"/>.</summary>
[ExcludeFromCodeCoverage]
public sealed class CapturingLogger(LogLevel minLevel) : ILogger
{
    public LogLevel MinLevel { get; } = minLevel;

    public List<string> Messages { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= MinLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
            Messages.Add(formatter(state, exception));
    }
}
