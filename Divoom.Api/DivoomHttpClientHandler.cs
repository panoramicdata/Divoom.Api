using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api;

internal sealed class DivoomHttpClientHandler(
	DivoomClientOptions options,
	DivoomClient DivoomClient,
	ILogger logger) : HttpClientHandler
{
	private readonly DivoomClientOptions _options = options;
	private readonly DivoomClient _divoomClient = DivoomClient;
	private readonly ILogger _logger = logger;
	private readonly LogLevel _levelToLogAt = LogLevel.Trace;

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		var logPrefix = $"Request {Guid.NewGuid()}: ";

		var attemptCount = 0;
		while (true)
		{
			attemptCount++;
			cancellationToken.ThrowIfCancellationRequested();

			await LogRequestAsync(logPrefix, request, cancellationToken).ConfigureAwait(false);

			// Complete the action
			var httpResponseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

			await LogResponseAsync(logPrefix, httpResponseMessage, cancellationToken).ConfigureAwait(false);

			var statusCodeInt = (int)httpResponseMessage.StatusCode;
			if (!TryGetBackOffDelay(logPrefix, request, httpResponseMessage, statusCodeInt, attemptCount, out var delay))
			{
				// Not a back-off request, so any further StatusCode handling is up to the caller
				return httpResponseMessage;
			}

			// Try up to the maximum retry count.
			if (attemptCount >= _options.HttpMaxAttemptCount)
			{
				if (_logger.IsEnabled(LogLevel.Information))
				{
					_logger.LogInformation(
						"{LogPrefix}Giving up retrying.  Returning {StatusCodeInt} on attempt {AttemptCount}/{MaxAttemptCount}.",
						logPrefix, statusCodeInt, attemptCount, _options.HttpMaxAttemptCount
						);
				}

				return httpResponseMessage;
			}

			if (_logger.IsEnabled(LogLevel.Information))
			{
				_logger.LogInformation(
					"{LogPrefix}Received {StatusCode} on attempt {AttemptCount}/{MaxAttemptCount} - Waiting {TotalSeconds:N2}s. ({Method} - {Url})",
					logPrefix,
					statusCodeInt,
					attemptCount,
					_options.HttpMaxAttemptCount,
					delay.TotalSeconds,
					request.Method.ToString(),
					request.RequestUri
					);
			}

			await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
		}
	}

	// Only do diagnostic logging if we're at the level we want to enable for as this is more efficient
	private async Task LogRequestAsync(
		string logPrefix,
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		if (!_logger.IsEnabled(_levelToLogAt))
		{
			return;
		}

		_logger.Log(_levelToLogAt, "{LogPrefix}Request\r\n{Request}", logPrefix, request);
		if (request.Content != null)
		{
			var requestContent = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			_logger.Log(_levelToLogAt, "{LogPrefix}RequestContent\r\n{RequestContent}", logPrefix, requestContent);
		}
	}

	// Only do diagnostic logging if we're at the level we want to enable for as this is more efficient
	private async Task LogResponseAsync(
		string logPrefix,
		HttpResponseMessage httpResponseMessage,
		CancellationToken cancellationToken)
	{
		if (!_logger.IsEnabled(_levelToLogAt))
		{
			return;
		}

		_logger.Log(_levelToLogAt, "{LogPrefix}Response\r\n{HttpResponseMessage}", logPrefix, httpResponseMessage);
		if (httpResponseMessage.Content != null)
		{
			var responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			_logger.Log(_levelToLogAt, "{LogPrefix}ResponseContent\r\n{ResponseContent}", logPrefix, responseContent);
		}
	}

	/// <summary>
	/// Determines whether the response asks us to back off and retry.
	/// </summary>
	/// <returns>
	/// <c>true</c> with the delay to wait, or <c>false</c> when the response should be returned to the caller.
	/// </returns>
	private bool TryGetBackOffDelay(
		string logPrefix,
		HttpRequestMessage request,
		HttpResponseMessage httpResponseMessage,
		int statusCodeInt,
		int attemptCount,
		out TimeSpan delay)
	{
		switch (statusCodeInt)
		{
			case 429:
				delay = TimeSpan.FromSeconds(1.1 * GetRetryAfterSeconds(httpResponseMessage));
				if (_logger.IsEnabled(LogLevel.Debug))
				{
					_logger.LogDebug(
						"{LogPrefix}Received {StatusCodeInt} on attempt {AttemptCount}/{MaxAttemptCount}.",
						logPrefix, statusCodeInt, attemptCount, _options.HttpMaxAttemptCount
						);
				}

				return true;
			case 502:
				if (_logger.IsEnabled(LogLevel.Information))
				{
					_logger.LogInformation(
						"{LogPrefix}Received {StatusCodeInt} on attempt {AttemptCount}/{MaxAttemptCount}.",
						logPrefix, statusCodeInt, attemptCount, _options.HttpMaxAttemptCount
						);
				}

				delay = TimeSpan.FromSeconds(5);
				return true;
			default:
				delay = default;
				LogFinalAttempt(logPrefix, request, statusCodeInt, attemptCount);
				return false;
		}
	}

	private void LogFinalAttempt(
		string logPrefix,
		HttpRequestMessage request,
		int statusCodeInt,
		int attemptCount)
	{
		if (attemptCount > 1 && _logger.IsEnabled(LogLevel.Debug))
		{
			_logger.LogDebug(
				"{LogPrefix}Received {StatusCodeInt} on attempt {AttemptCount}/{MaxAttemptCount}.",
				logPrefix, statusCodeInt, attemptCount, _options.HttpMaxAttemptCount
				);
		}

		if (statusCodeInt == 500)
		{
			_logger.LogError(
				"{LogPrefix}Received remote error code 500 on attempt {AttemptCount}/{MaxAttemptCount}. ({Method} - {Url})",
				logPrefix,
				attemptCount,
				_options.HttpMaxAttemptCount,
				request.Method.ToString(),
				request.RequestUri
				);
		}
	}

	// Back off by the requested amount, defaulting to a second when the header is absent or unparseable.
	private static int GetRetryAfterSeconds(HttpResponseMessage httpResponseMessage)
	{
		var foundHeader = httpResponseMessage.Headers.TryGetValues("Retry-After", out var retryAfterHeaders);
		var retryAfterSecondsString = foundHeader
			? retryAfterHeaders?.FirstOrDefault() ?? "1"
			: "1";

		return int.TryParse(retryAfterSecondsString, out var retryAfterSeconds)
			? retryAfterSeconds
			: 1;
	}
}
