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
			var deviceResponse = await Client.Bluetooth.SetBrightnessAsync(device, brightness, default);
		}
	}

	[Fact]
	public async void ViewTime_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client.Bluetooth.ViewTimeAsync(
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
	public async void ViewLightning_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client.Bluetooth.ViewLightningAsync(
			device,
			Color.Red,
			100,
			LightningType.PlainColor,
			default);
	}

	[Fact]
	public async void ViewCloudChannel_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client.Bluetooth.ViewCloudChannelAsync(device, default);
	}

	[Fact]
	public async void ViewVjEffects_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponses = new List<DeviceResponse>();
		foreach (var vjEffectType in Enum.GetValues<VjEffectType>())
		{
			var deviceResponse = await Client.Bluetooth.ViewVjEffectAsync(
				device,
				vjEffectType,
				default);
			deviceResponses.Add(deviceResponse);
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
			deviceResponses.Add(deviceResponse);
			await Task.Delay(1000);
		}
	}

	[Fact]
	public async void ViewAnimation_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client.Bluetooth.ViewAnimationAsync(device, default);
	}

	[Fact]
	public async void ViewWeather_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client.Bluetooth.ViewWeatherAsync(device, default);
	}

	[Fact]
	public async void SetDateTime_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client.Bluetooth.SetDateTimeAsync(device, DateTime.UtcNow.AddHours(1), default);
	}

	[Fact]
	public async void SetTemperatureAndWeather_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client.Bluetooth.SetTemperatureAndWeatherAsync(device, -1, WeatherType.Thunderstorm, default);
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

				await Task.Delay(1000);
			}
		}
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
