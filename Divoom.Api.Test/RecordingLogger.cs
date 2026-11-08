using Microsoft.Extensions.Logging;

namespace Divoom.Api.Test;

/// <summary>
/// A logger that records each entry at or above a minimum level, so tests can assert on what was logged.
/// </summary>
internal sealed class RecordingLogger(LogLevel minimumLevel = LogLevel.Trace) : ILogger
{
	private readonly List<(LogLevel Level, string Message)> _entries = [];

	public IReadOnlyList<(LogLevel Level, string Message)> Entries
	{
		get
		{
			lock (_entries)
			{
				return [.. _entries];
			}
		}
	}

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

	public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && logLevel >= minimumLevel;

	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter)
	{
		lock (_entries)
		{
			_entries.Add((logLevel, formatter(state, exception)));
		}
	}

	public IEnumerable<string> MessagesAt(LogLevel level) => Entries.Where(e => e.Level == level).Select(e => e.Message);
}
