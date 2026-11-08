using AwesomeAssertions;
using Divoom.Api.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace Divoom.Api.Test;

public sealed class ReadOnlyOptionTests : IDisposable
{
	private readonly FakeDevice _device = new();

	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	private DivoomClient CreateClient(bool isReadOnly) => new(
		new DivoomClientOptions
		{
			DeviceIp = $"127.0.0.1:{_device.Port}",
			IsReadOnly = isReadOnly,
			HttpMaxAttemptCount = 1
		},
		NullLogger.Instance);

	[Fact]
	public void IsReadOnly_DefaultsToFalse()
		=> new DivoomClientOptions().IsReadOnly.Should().BeFalse();

	[Fact]
	public async Task LocalWriteCommand_WhenReadOnly_ThrowsAndSendsNothing()
	{
		using var client = CreateClient(isReadOnly: true);

		var act = () => client.Channel.SetAsync(new SetChannelRequest { Index = 1 }, CancellationToken);

		// Refit wraps handler exceptions in ApiRequestException
		(await act.Should().ThrowAsync<Refit.ApiRequestException>()).WithInnerException<InvalidOperationException>();
		_device.Requests.Should().BeEmpty();
	}

	[Fact]
	public async Task LocalWriteCommand_WhenNotReadOnly_IsSent()
	{
		using var client = CreateClient(isReadOnly: false);

		var response = await client.Channel.SetDialAsync(new SetDialRequest { DialId = 1 }, CancellationToken);

		response.Should().NotBeNull();
		_device.Requests.Should().ContainSingle().Which.Should().Contain("Channel/SetClockSelectId");
	}

	[Fact]
	public async Task LocalGetCommand_WhenReadOnly_IsStillSent()
	{
		using var client = CreateClient(isReadOnly: true);

		var response = await client.Channel.GetDialInfoAsync(new(), CancellationToken);

		response.Brightness.Should().Be(42);
		_device.Requests.Should().ContainSingle().Which.Should().Contain("Channel/GetClockInfo");
	}

	[Fact]
	public async Task BluetoothWriteCommand_WhenReadOnly_Throws()
	{
		using var client = CreateClient(isReadOnly: true);

		var act = () => client.Bluetooth.Settings.SetBrightnessAsync(new DivoomBluetoothDevice(null!), 50, CancellationToken);

		await act.Should().ThrowAsync<InvalidOperationException>();
	}

	public void Dispose()
	{
		_device.Dispose();
		GC.SuppressFinalize(this);
	}
}
