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
	private readonly DivoomClient _DivoomClient = DivoomClient;
	private readonly ILogger _logger = logger;
	private readonly LogLevel _levelToLogAt = LogLevel.Trace;

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		var logPrefix = $"Request {Guid.NewGuid()}: ";

		// Add the request headers
		var attemptCount = 0;
		while (true)
		{
			attemptCount++;
			cancellationToken.ThrowIfCancellationRequested();

			// Only do diagnostic logging if we're at the level we want to enable for as this is more efficient
			if (_logger.IsEnabled(_levelToLogAt))
			{
				_logger.Log(_levelToLogAt, "{LogPrefix}Request\r\n{Request}", logPrefix, request);
				if (request.Content != null)
				{
					var requestContent = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
					_logger.Log(_levelToLogAt, "{LogPrefix}RequestContent\r\n{RequestContent}", logPrefix, requestContent);
				}
			}

			// Complete the action
			var httpResponseMessage = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

			// Only do diagnostic logging if we're at the level we want to enable for as this is more efficient
			if (_logger.IsEnabled(_levelToLogAt))
			{
				_logger.Log(_levelToLogAt, "{LogPrefix}Response\r\n{HttpResponseMessage}", logPrefix, httpResponseMessage);
				if (httpResponseMessage.Content != null)
				{
					var responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
					_logger.Log(_levelToLogAt, "{LogPrefix}ResponseContent\r\n{ResponseContent}", logPrefix, responseContent);
				}
			}

			TimeSpan delay;
			// As long as we were not given a back-off request then we'll return the response and any further StatusCode handling is up to the caller
			var statusCodeInt = (int)httpResponseMessage.StatusCode;
			switch (statusCodeInt)
			{
				case 429:
					// Back off by the requested amount.
					var headers = httpResponseMessage.Headers;
					var foundHeader = headers.TryGetValues("Retry-After", out var retryAfterHeaders);
					var retryAfterSecondsString = foundHeader
						? retryAfterHeaders.FirstOrDefault() ?? "1"
						: "1";
					if (!int.TryParse(retryAfterSecondsString, out var retryAfterSeconds))
					{
						retryAfterSeconds = 1;
					}

					delay = TimeSpan.FromSeconds(1.1 * retryAfterSeconds);
					_logger.LogDebug(
						"{LogPrefix}Received {StatusCodeInt} on attempt {AttemptCount}/{MaxAttemptCount}.",
						logPrefix, statusCodeInt, attemptCount, _options.HttpMaxAttemptCount
						);
					break;
				case 502:
					_logger.LogInformation(
						"{LogPrefix}Received {StatusCodeInt} on attempt {AttemptCount}/{MaxAttemptCount}.",
						logPrefix, statusCodeInt, attemptCount, _options.HttpMaxAttemptCount
						);
					delay = TimeSpan.FromSeconds(5);
					break;
				default:
					if (attemptCount > 1)
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

					return httpResponseMessage;
			}
			// Try up to the maximum retry count.
			if (attemptCount >= _options.HttpMaxAttemptCount)
			{
				_logger.LogInformation(
					"{LogPrefix}Giving up retrying.  Returning {StatusCodeInt} on attempt {AttemptCount}/{MaxAttemptCount}.",
					logPrefix, statusCodeInt, attemptCount, _options.HttpMaxAttemptCount
					);
				return httpResponseMessage;
			}

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
			await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
		}
	}
}
