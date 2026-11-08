using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Refit;
using System.Net;

namespace Divoom.Api.Test;

public sealed class HttpClientHandlerTests : IDisposable
{
	private readonly FakeDevice _device = new();
	private readonly RecordingLogger _logger = new();

	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	private DivoomClient CreateClient(int maxAttemptCount) => new(
		new DivoomClientOptions
		{
			DeviceIp = $"127.0.0.1:{_device.Port}",
			HttpMaxAttemptCount = maxAttemptCount
		},
		_logger);

	[Fact]
	public async Task TooManyRequests_ThenSuccess_IsRetriedAfterRetryAfter()
	{
		_device.Status = index => index == 0 ? (429, "Retry-After: 0\r\n") : (200, string.Empty);
		using var client = CreateClient(maxAttemptCount: 2);

		var response = await client.Channel.GetDialInfoAsync(new(), CancellationToken);

		response.Brightness.Should().Be(42);
		_device.Requests.Should().HaveCount(2);
		_logger.MessagesAt(LogLevel.Information).Should().ContainSingle()
			.Which.Should().Contain("Received 429 on attempt 1/2").And.Contain("Waiting 0.00s");
		_logger.MessagesAt(LogLevel.Debug).Should().Contain(m => m.Contains("Received 429 on attempt 1/2", StringComparison.Ordinal));
		_logger.MessagesAt(LogLevel.Debug).Should().Contain(m => m.Contains("Received 200 on attempt 2/2", StringComparison.Ordinal));
	}

	[Theory]
	[InlineData("")]
	[InlineData("Retry-After: soon\r\n")]
	public async Task TooManyRequests_WithoutUsableRetryAfter_WaitsOnePointOneSeconds(string headers)
	{
		_device.Status = index => index == 0 ? (429, headers) : (200, string.Empty);
		using var client = CreateClient(maxAttemptCount: 2);

		await client.Channel.GetDialInfoAsync(new(), CancellationToken);

		_device.Requests.Should().HaveCount(2);
		_logger.MessagesAt(LogLevel.Information).Should().ContainSingle().Which.Should().Contain("Waiting 1.10s");
	}

	[Fact]
	public async Task TooManyRequests_OnLastAttempt_GivesUpAndReturnsTheStatus()
	{
		_device.Status = _ => (429, "Retry-After: 0\r\n");
		using var client = CreateClient(maxAttemptCount: 1);

		var act = () => client.Channel.GetDialInfoAsync(new(), CancellationToken);

		(await act.Should().ThrowAsync<ApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
		_device.Requests.Should().ContainSingle();
		_logger.MessagesAt(LogLevel.Information).Should().ContainSingle()
			.Which.Should().Contain("Giving up retrying.  Returning 429 on attempt 1/1.");
	}

	[Fact]
	public async Task BadGateway_OnLastAttempt_GivesUpWithoutWaiting()
	{
		_device.Status = _ => (502, string.Empty);
		using var client = CreateClient(maxAttemptCount: 1);

		var act = () => client.Channel.GetDialInfoAsync(new(), CancellationToken);

		(await act.Should().ThrowAsync<ApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.BadGateway);
		_device.Requests.Should().ContainSingle();
		_logger.MessagesAt(LogLevel.Information).Should().HaveCount(2)
			.And.Contain(m => m.Contains("Received 502 on attempt 1/1.", StringComparison.Ordinal))
			.And.Contain(m => m.Contains("Giving up retrying.  Returning 502 on attempt 1/1.", StringComparison.Ordinal));
	}

	[Fact]
	public async Task InternalServerError_IsLoggedAsAnErrorAndNotRetried()
	{
		_device.Status = _ => (500, string.Empty);
		using var client = CreateClient(maxAttemptCount: 3);

		var act = () => client.Channel.GetDialInfoAsync(new(), CancellationToken);

		(await act.Should().ThrowAsync<ApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
		_device.Requests.Should().ContainSingle();
		_logger.MessagesAt(LogLevel.Error).Should().ContainSingle()
			.Which.Should().Contain("Received remote error code 500 on attempt 1/3").And.Contain("POST");
	}

	[Fact]
	public async Task TraceLogging_RecordsRequestAndResponseContent()
	{
		using var client = CreateClient(maxAttemptCount: 1);

		await client.Channel.GetDialInfoAsync(new(), CancellationToken);

		var trace = _logger.MessagesAt(LogLevel.Trace).ToList();
		trace.Should().Contain(m => m.Contains("RequestContent", StringComparison.Ordinal) && m.Contains("Channel/GetClockInfo", StringComparison.Ordinal));
		trace.Should().Contain(m => m.Contains("ResponseContent", StringComparison.Ordinal) && m.Contains("\"Brightness\":42", StringComparison.Ordinal));
	}

	[Fact]
	public async Task LoggingBelowTrace_RecordsNoContent()
	{
		var logger = new RecordingLogger(LogLevel.Debug);
		using var client = new DivoomClient(
			new DivoomClientOptions { DeviceIp = $"127.0.0.1:{_device.Port}", HttpMaxAttemptCount = 1 },
			logger);

		await client.Channel.GetDialInfoAsync(new(), CancellationToken);

		logger.Entries.Should().BeEmpty();
	}

	public void Dispose() => _device.Dispose();
}
