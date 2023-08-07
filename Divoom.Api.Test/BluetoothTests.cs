using Divoom.Api.Models;
using FluentAssertions;
using System.Drawing;
using Xunit.Abstractions;

namespace Divoom.Api.Test;

public class BluetoothTests : Test
{
	public BluetoothTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
	{
	}

	[Fact]
	public async Task GetDivoomDevice_Succeeds()
	{
		var device = GetFirstDevice();
		device.Should().BeOfType<DivoomBluetoothDevice>();
	}

	[Fact]
	public async Task SetBrightness_Succeeds()
	{
		var device = GetFirstDevice();
		// Set the brightness from 0% to 100% in steps of 10
		for (var brightness = 0; brightness <= 100; brightness += 10)
		{
			_ = await Client.Bluetooth.SetBrightnessAsync(device, brightness, default);
		}
	}

	[Fact]
	public async Task SetVolume_To2Then3_Succeeds()
	{
		var device = GetFirstDevice();

		await Client
			.Bluetooth
			.SetVolumeAsync(device, 2, default);

		var volumeRefetch = await Client
			.Bluetooth
			.GetVolumeAsync(device, default);

		volumeRefetch.Should().Be(2);

		await Client
			.Bluetooth
			.SetVolumeAsync(device, 3, default);

		volumeRefetch = await Client
			.Bluetooth
			.GetVolumeAsync(device, default);

		volumeRefetch.Should().Be(3);
	}

	[Fact]
	public async Task SetVolume_ToValuesOtherThan3_Succeeds()
	{
		var device = GetFirstDevice();
		// Set the volume from 0 to 16
		for (var volume = 0; volume <= 16; volume++)
		{
			if (volume == 3)
			{
				continue;
			}

			await Client
				.Bluetooth
				.SetVolumeAsync(device, volume, default);

			var volumeRefetch = await Client
				.Bluetooth
				.GetVolumeAsync(device, default);

			switch (volume)
			{
				case 16:
					volumeRefetch.Should().Be(15);
					break;
				default:
					volumeRefetch.Should().Be(volume);
					break;
			}
		}
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(17)]
	public async Task SetVolume_Fails_OutsideRange(int illegalVolume)
	{
		var device = GetFirstDevice();
		try
		{
			await Client
				.Bluetooth
				.SetVolumeAsync(device, illegalVolume, default);
			throw new InvalidOperationException("Should have thrown an exception");
		}
		catch (ArgumentOutOfRangeException)
		{
			return;
		}
	}

	[Fact]
	public async Task GetVolume_Succeeds()
	{
		var device = GetFirstDevice();
		var volume = await Client
			.Bluetooth
			.GetVolumeAsync(device, default);

		volume.Should().BeInRange(0, 16);
	}

	[Fact]
	public async Task ViewTime_Succeeds()
	{
		var device = GetFirstDevice();
		_ = await Client.Bluetooth.ViewTimeAsync(
			device,
			TimeType.TwentyFourHours,
			ClockType.FullScreenNegative,
			true,
			false,
			false,
			false,
			Color.Red,
			default);
	}

	[Fact]
	public async Task SetWeather_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client.Bluetooth.SetWeatherAsync(
			device,
			Color.Red,
			100,
			WeatherType.Snow,
			default);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewCloudChannel_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client.Bluetooth.ViewCloudChannelAsync(device, default);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewVjEffects_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponses = new List<DeviceResponse>();
		foreach (var vjEffectType in Enum.GetValues<VjEffectType>())
		{
			var deviceResponse = await Client.Bluetooth.ViewVjEffectAsync(
				device,
				vjEffectType,
				default);

			deviceResponse.IsOk.Should().BeTrue();

			deviceResponses.Add(deviceResponse);
			await Task.Delay(1000);
		}
	}

	[Fact]
	public async Task ViewVisualization_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponses = new List<DeviceResponse>();
		foreach (var visualizationType in Enum.GetValues<VisualizationType>())
		{
			var deviceResponse = await Client.Bluetooth.ViewVisualizationAsync(
				device,
				visualizationType,
				default);

			deviceResponse.IsOk.Should().BeTrue();

			deviceResponses.Add(deviceResponse);
			await Task.Delay(1000);
		}
	}

	[Fact]
	public async Task ViewAnimation_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client.Bluetooth.ViewAnimationAsync(device, default);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewWeather_Succeeds()
	{
		var device = GetFirstDevice();

		var deviceResponse = await Client.Bluetooth.SetTemperatureAndWeatherAsync(device, -1, WeatherType.Thunderstorm, default);

		deviceResponse = await Client.Bluetooth.ViewWeatherAsync(device, default);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task GetSettings_Succeeds()
	{
		var device = GetFirstDevice();

		_ = await Client.Bluetooth.GetSettingsAsync(device, default);
	}

	[Fact]
	public async Task SetDateTime_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client.Bluetooth.SetDateTimeAsync(device, DateTime.UtcNow.AddHours(1), default);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task SetTemperatureAndWeather_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client.Bluetooth.SetTemperatureAndWeatherAsync(device, -1, WeatherType.Thunderstorm, default);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewScoreboard_Succeeds()
	{
		var device = GetFirstDevice();

		for (var redScore = 0; redScore <= 4; redScore++)
		{
			for (var blueScore = 0; blueScore <= 4; blueScore++)
			{
				var deviceResponse = await Client
					.Bluetooth
					.ViewScoreboardAsync(
						device,
						redScore,
						blueScore, default);

				deviceResponse.IsOk.Should().BeTrue();

				await Task.Delay(1000);
			}
		}
	}

	[Fact]
	public async Task ViewImageSucceeds()
	{
		var image = new Color[256];
		var pixelIndex = 0;
		for (var x = 0; x < 16; x++)
		{
			for (var y = 0; y < 16; y++)
			{
				var r = x % 2 == 0 ? 0x00 : 0xff;
				var g = y % 2 == 0 ? 0x00 : 0xff;
				var b = (byte)(x * 16);
				image[pixelIndex++] = Color.FromArgb(r, g, b);
			}
		}

		var device = GetFirstDevice();

		var deviceResponse = await Client
			.Bluetooth
			.ViewImageAsync(
				device,
				image,
				default
			);

		deviceResponse.IsOk.Should().BeTrue();
	}

	private DivoomBluetoothDevice GetFirstDevice()
	{
		var devices = Client.Bluetooth.GetDevices();
		devices.Should().NotBeNull();
		devices.Should().BeOfType<List<DivoomBluetoothDevice>>();
		devices.Should().HaveCountGreaterThan(0);

		return devices[0];
	}
}
