using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace W65C02S.Engine.Tests;

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

[ExcludeFromCodeCoverage]
public class UnitTestFormatterOptions : ConsoleFormatterOptions
{
    public static char DefaultPrefixChar = ' ';

    public int IndentSize { get; set; } = 2;
    public char PrefixChar { get; set; } = DefaultPrefixChar;
}

[ExcludeFromCodeCoverage]
public static class ConsoleLoggerExtensions
{
    public static ILoggingBuilder AddConsoleIndentLogger(this ILoggingBuilder builder, Action<UnitTestFormatterOptions> configure) =>
        builder
            .AddConsole(options => options.FormatterName = UnitTestFormatter.FormatterName)
            .AddConsoleFormatter<UnitTestFormatter, UnitTestFormatterOptions>(configure);
}
