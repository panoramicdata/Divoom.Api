using Microsoft.Extensions.Logging;

namespace Divoom.Api.Test;

internal sealed class XunitLogger(ITestOutputHelper output, string categoryName) : ILogger
{
	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

	public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

	public void Log<TState>(
		LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
		{
			return;
		}

		var message = formatter(state, exception);
		output.WriteLine($"[{logLevel}] {categoryName}: {message}");

		if (exception is not null)
		{
			output.WriteLine(exception.ToString());
		}
	}
}

internal sealed class XunitLoggerProvider(ITestOutputHelper output) : ILoggerProvider
{
	public ILogger CreateLogger(string categoryName) => new XunitLogger(output, categoryName);

	public void Dispose() { }
}
