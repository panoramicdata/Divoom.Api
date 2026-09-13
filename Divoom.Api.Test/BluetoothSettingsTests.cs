using AwesomeAssertions;
using Divoom.Api.Models;

namespace Divoom.Api.Test;

/// <summary>
/// Bluetooth tests covering brightness, volume, mute, temperature unit and date/time settings.
/// </summary>
[Collection("Bluetooth")]
public class BluetoothSettingsTests(ITestOutputHelper testOutputHelper, BluetoothFixture fixture)
	: BluetoothTestBase(testOutputHelper, fixture)
{
	[Fact]
	public async Task SetBrightness_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		// Set the brightness from 0% to 100% in steps of 10
		for (var brightness = 0; brightness <= 100; brightness += 10)
		{
			var deviceResponse = await Client
				.Bluetooth
				.Settings
				.SetBrightnessAsync(device, brightness, CancellationToken);

			deviceResponse.IsOk.Should().BeTrue();
		}
	}

	[Fact]
	public async Task SetVolume_To3_SetsTo2()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);

		await Client
			.Bluetooth
			.Settings
			.SetVolumeAsync(device, 7, CancellationToken);

		var volumeRefetch = await Client
			.Bluetooth
			.Settings
			.GetVolumeAsync(device, CancellationToken);

		volumeRefetch.Should().Be(7);

		await Client
			.Bluetooth
			.Settings
			.SetVolumeAsync(device, 3, CancellationToken);

		volumeRefetch = await Client
			.Bluetooth
			.Settings
			.GetVolumeAsync(device, CancellationToken);

		volumeRefetch.Should().Be(2);
	}

	[Fact]
	public async Task SetVolume_ToValuesOtherThan3_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		// Set the volume from 0 to 16
		for (var volume = 0; volume <= 16; volume++)
		{
			if (volume == 3)
			{
				continue;
			}

			await Client
				.Bluetooth
				.Settings
				.SetVolumeAsync(device, volume, CancellationToken);

			var volumeRefetch = await Client
				.Bluetooth
				.Settings
				.GetVolumeAsync(device, CancellationToken);

			if (volume == 16)
			{
				volumeRefetch.Should().Be(15);
			}
			else
			{
				volumeRefetch.Should().Be(volume);
			}
		}
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(17)]
	public async Task SetVolume_Fails_OutsideRange(int illegalVolume)
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		try
		{
			await Client
				.Bluetooth
				.Settings
				.SetVolumeAsync(device, illegalVolume, CancellationToken);
			throw new InvalidOperationException("Should have thrown an exception");
		}
		catch (ArgumentOutOfRangeException)
		{
			//To stop codacy complaining about empty catch blocks
			_ = 0;
		}
	}

	[Fact]
	public async Task GetVolume_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		var volume = await Client
			.Bluetooth
			.Settings
			.GetVolumeAsync(device, CancellationToken);

		volume.Should().BeInRange(0, 16);
	}

	[Fact]
	public async Task GetOutput_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		var deviceResponseSet = await Client
			.Bluetooth
			.ReadResponseAsync(device, TimeSpan.FromMilliseconds(5000), CancellationToken);
		_ = deviceResponseSet.Should().NotBeNull();
	}

	[Fact]
	public async Task GetMuteState_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		var muteState = await Client
			.Bluetooth
			.Settings
			.GetMuteStateAsync(device, CancellationToken);
		muteState.Should().BeOneOf(MuteState.Muted, MuteState.Unmuted);
	}

	[Fact]
	public async Task SetMuteState_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);

		await Client
			.Bluetooth
			.Settings
			.SetMuteStateAsync(device,
				MuteState.Muted,
				CancellationToken);
	}

	[Fact]
	public async Task SetTemperatureUnit_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);

		await Client
			.Bluetooth
			.Settings
			.SetTemperatureUnitAsync(device,
				TemperatureUnit.Farenheit,
				CancellationToken);

		await Client
			.Bluetooth
			.Settings
			.SetTemperatureUnitAsync(device,
				TemperatureUnit.Celsius,
				CancellationToken);
	}

	[Fact]
	public async Task GetSettings_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);

		_ = await Client
			.Bluetooth
			.Settings
			.GetSettingsAsync(device, CancellationToken);
	}

	[Fact]
	public async Task SetDateTime_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		await Client
			.Bluetooth
			.Settings
			.SetDateTimeAsync(
				device,
				DateTime.UtcNow.AddHours(1),
				CancellationToken);
	}
}
