using AwesomeAssertions;
using Divoom.Api.Exceptions;
using Divoom.Api.Implementations;
using Divoom.Api.Models;

namespace Divoom.Api.Test;

public class BluetoothSettingsNoAnswerTests
{
	private static readonly DivoomBluetoothDevice Device = new(null!);

	[Fact]
	public async Task GetSettings_SendsTheGetSettingsCommand()
	{
		var transport = new StubTransport([]);
		var settings = new BluetoothSettingsCommands(transport);

		_ = await Assert.ThrowsAsync<DivoomNoResponseException>(() => settings.GetSettingsAsync(Device, CancellationToken.None));

		transport.Sent.Should().Equal((byte)Command.GetSettings);
	}

	[Fact]
	public async Task GetSettings_WithNoResponses_ThrowsNoResponse()
	{
		var settings = new BluetoothSettingsCommands(new StubTransport([]));

		var act = () => settings.GetSettingsAsync(Device, CancellationToken.None);

		await act.Should().ThrowAsync<DivoomNoResponseException>();
	}

	[Fact]
	public async Task GetSettings_WithAResponseCarryingNoData_ThrowsNoResponse()
	{
		var response = new DeviceResponse([(byte)Command.Response, (byte)Command.GetSettings, 0x55]);
		var settings = new BluetoothSettingsCommands(new StubTransport([response]));

		var act = () => settings.GetSettingsAsync(Device, CancellationToken.None);

		await act.Should().ThrowAsync<DivoomNoResponseException>();
	}

	[Fact]
	public async Task GetSettings_WithData_ParsesTheSettings()
	{
		var response = new DeviceResponse([(byte)Command.Response, (byte)Command.GetSettings, 0x55, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x64]);
		var settings = new BluetoothSettingsCommands(new StubTransport([response]));

		var result = await settings.GetSettingsAsync(Device, CancellationToken.None);

		result.BrightnessPercent.Should().Be(100);
	}

	private sealed class StubTransport(List<DeviceResponse> responses) : IDeviceTransport
	{
		public List<byte> Sent { get; } = [];

		public Task<DeviceResponseSet> SendCommandAsync(
			DivoomBluetoothDevice device,
			CancellationToken cancellationToken,
			params byte[] commandBytes)
		{
			ArgumentNullException.ThrowIfNull(device);
			cancellationToken.ThrowIfCancellationRequested();
			Sent.AddRange(commandBytes);
			return Task.FromResult(new DeviceResponseSet(responses));
		}
	}
}
