using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Sim6502.Tests.Logging;

[ExcludeFromCodeCoverage]
public class UnitTestFormatter : ConsoleFormatter
{
    public const string FormatterName = nameof(UnitTestFormatter);

    public UnitTestFormatter(IOptionsMonitor<UnitTestFormatterOptions> options)
        : base(FormatterName)
    {
        options.OnChange(ReloadLoggerOptions);
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

        textWriter.WriteLine(message);
    }

    private void ReloadLoggerOptions(UnitTestFormatterOptions options)
    {
    }
}

public class UnitTestFormatterOptions : ConsoleFormatterOptions
{
    public static char DefaultPrefixChar = ' ';

    public int IndentSize { get; set; } = 2;
    public char PrefixChar { get; set; } = DefaultPrefixChar;
}

public static class ConsoleLoggerExtensions
{
    public static ILoggingBuilder AddConsoleIndentLogger(this ILoggingBuilder builder, Action<UnitTestFormatterOptions> configure) =>
        builder
            .AddConsole(options => options.FormatterName = UnitTestFormatter.FormatterName)
            .AddConsoleFormatter<UnitTestFormatter, UnitTestFormatterOptions>(configure);
}
