using Divoom.Api.Models;
using FluentAssertions;
using System.Drawing;
using Xunit.Abstractions;

namespace Divoom.Api.Test
{
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
				Client.Bluetooth.SetBrightness(device, brightness);
				await Task.Delay(100);
			}
		}

		[Fact]
		public async void ViewTime_Succeeds()
		{
			var device = GetFirstDevice();
			Client.Bluetooth.ViewTime(
				device,
				TimeType.TwentyFourHours,
				ClockType.FullScreenNegative,
				true,
				true,
				true,
				true,
				Color.Red);
		}

		[Fact]
		public async void ViewLightning_Succeeds()
		{
			var device = GetFirstDevice();
			Client.Bluetooth.ViewLightning(
				device,
				Color.Red,
				100,
				LightningType.PlainColor);
		}

		[Fact]
		public async void ViewCloudChannel_Succeeds()
		{
			var device = GetFirstDevice();
			Client.Bluetooth.ViewCloudChannel(device);
		}

		[Fact]
		public async void ViewVjEffects_Succeeds()
		{
			var device = GetFirstDevice();
			Client.Bluetooth.ViewVjEffects(device);
		}

		[Fact]
		public async void ViewVisualization_Succeeds()
		{
			var device = GetFirstDevice();

			foreach (var visualizationType in Enum.GetValues<VisualizationType>())
			{
				Client.Bluetooth.ViewVisualization(
					device,
					visualizationType);

				await Task.Delay(1000);
			}
		}

		[Fact]
		public async void ViewAnimation_Succeeds()
		{
			var device = GetFirstDevice();
			Client.Bluetooth.ViewAnimation(device);
		}

		[Fact]
		public async void ViewWeather_Succeeds()
		{
			var device = GetFirstDevice();
			Client.Bluetooth.ViewWeather(device);
		}

		[Fact]
		public async void SetDateTime_Succeeds()
		{
			var device = GetFirstDevice();
			Client.Bluetooth.SetDateTime(device, DateTime.UtcNow.AddHours(1));
		}

		[Fact]
		public async void SetTemperatureAndWeather_Succeeds()
		{
			var device = GetFirstDevice();
			Client.Bluetooth.SetTemperatureAndWeather(device, -1, WeatherType.Thunderstorm);
		}

		[Fact]
		public async void ViewScoreboard_Succeeds()
		{
			var device = GetFirstDevice();

			for (var redScore = 0; redScore <= 4; redScore++)
			{
				for (var blueScore = 0; blueScore <= 4; blueScore++)
				{
					Client
						.Bluetooth
						.ViewScoreboard(
							device,
							redScore,
							blueScore);

					var responseBytes = Client.Bluetooth.ReadBytes(device);

					await Task.Delay(1000);
				}
			}
		}

		[Fact]
		public void ReadBytes_Succeeds()
		{
			var device = GetFirstDevice();

			var bytes = Client
				.Bluetooth
				.ReadBytes(device);

			bytes.Should().NotBeNullOrEmpty();
		}

		private DivoomBluetoothDevice GetFirstDevice()
		{
			var response = Client.Bluetooth.GetDevices();
			response.Should().NotBeNull();
			response.Should().BeOfType<List<DivoomBluetoothDevice>>();
			response.Should().HaveCountGreaterThan(0);

			return response[0];
		}
	}
}
