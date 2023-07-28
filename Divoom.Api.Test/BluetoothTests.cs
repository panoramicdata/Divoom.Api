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
	public async void GetDivoomDevice_Succeeds()
	{
		var device = GetFirstDevice();
		device.Should().BeOfType<DivoomBluetoothDevice>();
	}

	[Fact]
	public async void SetBrightness_Succeeds()
	{
		var device = GetFirstDevice();
		// Set the brightness from 0% to 100% in steps of 10
		for (var brightness = 0; brightness <= 100; brightness += 10)
		{
			var deviceResponse = await Client
				.Bluetooth
				.SetBrightnessAsync(device, brightness, default);

			deviceResponse.IsOk.Should().BeTrue();
		}
	}

	[Fact]
	public async void SetVolume_To2Then3_Succeeds()
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
	public async void SetVolume_ToValuesOtherThan3_Succeeds()
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
	public async void SetVolume_Fails_OutsideRange(int illegalVolume)
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
	public async void GetVolume_Succeeds()
	{
		var device = GetFirstDevice();
		var volume = await Client
			.Bluetooth
			.GetVolumeAsync(device, default);

		volume.Should().BeInRange(0, 16);
	}


	[Fact]
	public async void GetOutput_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponseSet = await Client
			.Bluetooth
			.ReadResponseAsync(device, TimeSpan.FromMilliseconds(5000), default);
	}

	[Fact]
	public async void GetMuteState_Succeeds()
	{
		var device = GetFirstDevice();
		var muteState = await Client
			.Bluetooth
			.GetMuteStateAsync(device, default);
	}

	[Fact]
	public async void SetMuteState_Succeeds()
	{
		var device = GetFirstDevice();

		await Client
			.Bluetooth
			.SetMuteStateAsync(device,
				MuteState.Muted,
				default);
	}

	[Fact]
	public async void SetTemperatureUnit_Succeeds()
	{
		var device = GetFirstDevice();

		await Client
			.Bluetooth
			.SetTemperatureUnitAsync(device,
				TemperatureUnit.Farenheit,
				default);

		var refetchedTemperatureUnit = await Client
			.Bluetooth
			.GetTemperatureUnitAsync(device, default);

		refetchedTemperatureUnit
			.Should()
			.Be(TemperatureUnit.Farenheit);

		await Client
			.Bluetooth
			.SetTemperatureUnitAsync(device,
				TemperatureUnit.Celsius,
				default);

		refetchedTemperatureUnit = await Client
			.Bluetooth
			.GetTemperatureUnitAsync(device, default);

		refetchedTemperatureUnit
			.Should()
			.Be(TemperatureUnit.Celsius);
	}

	[Fact]
	public async void SetColor_Succeeds()
	{
		var device = GetFirstDevice();

		await Client
			.Bluetooth
			.SetColorAsync(device,
				DivoomColor.Green,
				default);
	}

	[Fact]
	public async void GetWeather_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client
			.Bluetooth
			.GetWeatherAsync(device, default);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async void ViewTime_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponseSet = await Client
			.Bluetooth
			.ViewAllTheThingsAsync(
				device,
				TimeType.TwentyFourHours,
				ClockType.FullScreenNegative,
				false,
				true,
				false,
				false,
				Color.Red,
				100,
				default);
	}

	[Fact]
	public async void ViewAllTheThingsAsync_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client
			.Bluetooth
			.ViewAllTheThingsAsync(
				device,
				TimeType.TwentyFourHours,
				ClockType.FullScreenNegative,
				true,
				true,
				true,
				true,
				Color.Blue,
				100,
				default);

		deviceResponse.IsOk.Should().BeTrue();
	}


	[Fact]
	public async void SetWeather1_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client
			.Bluetooth
			.SetWeatherAsync(
				device,
				30,
				WeatherType.Fog,
				default
			);

		deviceResponse.IsOk.Should().BeTrue();
	}


	[Fact]
	public async void ViewChannel_Succeeds()
	{
		var device = GetFirstDevice();

		foreach (var channel in Enum.GetValues<Channel>())
		{
			var deviceResponseSet = await Client
				.Bluetooth
				.ViewChannelAsync(
					device,
					channel,
					default);

			await Task.Delay(1000);
		}
	}

	[Fact]
	public async void ViewCloudChannel_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client
			.Bluetooth
			.ViewCloudChannelAsync(device, default);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async void ViewColorChange_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client
			.Bluetooth
			.ViewColorChangeAsync(
				device,
				Color.Magenta,
				default);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async void ViewVjEffects_Succeeds()
	{
		var device = GetFirstDevice();
		foreach (var vjEffectType in Enum.GetValues<VjEffectType>())
		{
			var deviceResponse = await Client.Bluetooth.ViewVjEffectAsync(
				device,
				vjEffectType,
				default);

			deviceResponse.IsOk.Should().BeTrue();

			await Task.Delay(1000);
		}
	}

	[Fact]
	public async void ViewVisualization_Succeeds()
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

			await Task.Delay(1000);
		}
	}

	[Fact]
	public async void ViewAnimation_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client
			.Bluetooth
			.ViewAnimationAsync(device, default);

		deviceResponse.IsOk.Should().BeTrue();
	}
	[Fact]
	public async void ViewStopwatch_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client
			.Bluetooth
			.ViewStopwatchAsync(
				device,
				TimeSpan.FromMinutes(1),
				default);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async void ViewWeather_Succeeds()
	{
		var device = GetFirstDevice();

		var deviceResponse = await Client
			.Bluetooth
			.SetWeatherAsync(
				device,
				-1,
				WeatherType.Thunderstorm,
				default);

		deviceResponse.IsOk.Should().BeTrue();

		var deviceResponseSet = await Client
			.Bluetooth
			.ViewAllTheThingsAsync(
				device,
				TimeType.TwelveHours,
				ClockType.AnalogRound,
				true,
				false,
				false,
				false,
				Color.Blue,
				100,
				default);

		deviceResponseSet
			.IsOk
			.Should()
			.BeTrue();
	}

	[Fact]
	public async void GetSettings_Succeeds()
	{
		var device = GetFirstDevice();

		var deviceSettings = await Client.Bluetooth.GetSettingsAsync(device, default);
	}

	[Fact]
	public async void SetDateTime_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client
			.Bluetooth
			.SetDateTimeAsync(
				device,
				DateTime.UtcNow.AddHours(1),
				default);

		deviceResponse.IsOk.Should().BeTrue();
	}


	[Fact]
	public async void ViewScoreboard_Succeeds()
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
	public async void ViewImageSucceeds()
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
