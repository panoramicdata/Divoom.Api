﻿using Divoom.Api.Models;
using FluentAssertions;
using Xunit.Abstractions;
using Color = System.Drawing.Color;
using Image = SixLabors.ImageSharp.Image;

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
			.ViewClockAsync(
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
	public async void ViewClockAsync_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client
			.Bluetooth
			.ViewClockAsync(
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
	public async void ViewClockAsync_JustClock_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client
			.Bluetooth
			.ViewClock2Async(
				device,
				TimeType.TwelveHours,
				ClockType.FullScreen,
				false,
				false,
				false,
				true,
				Color.Yellow,
				default);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async void SetWeatherAsync_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponseSet = await Client
			.Bluetooth
			.SetWeatherAsync(
				device,
				30,
				WeatherType.Clear,
				default
			);

		deviceResponseSet.IsOk.Should().BeTrue();
	}


	[Fact]
	public async void ViewChannel_Succeeds()
	{
		var device = GetFirstDevice();

		foreach (var channel in Enum.GetValues<Models.Channel>())
		{
			var deviceResponseSet = await Client
				.Bluetooth
				.ViewChannelAsync(
					device,
					channel,
					default);

			await Task.Delay(1000);
		}

		var deviceResponseSet2 = await Client
			.Bluetooth
			.ViewChannelAsync(
				device,
				Models.Channel.Scoreboard,
				default);
	}

	[Fact]
	public async void ViewLighting_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client
			.Bluetooth
			.ViewLightingAsync(
				device,
				Color.Magenta,
				100,
				LightingPattern.Custom,
				PowerState.On,
				default);

		deviceResponse.IsOk.Should().BeTrue();
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
			.ViewClockAsync(
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
	public async void ViewImage_Constructed_Succeeds()
	{
		var imageBytes = new Color[256];
		var pixelIndex = 0;
		for (var x = 0; x < 16; x++)
		{
			for (var y = 0; y < 16; y++)
			{
				var r = pixelIndex;
				var g = x * 16;
				var b = y * 16;
				imageBytes[pixelIndex++] = Color.FromArgb(r, g, b);
			}
		}

		var device = GetFirstDevice();

		var deviceResponse = await Client
			.Bluetooth
			.ViewImageAsync(
				device,
				new DivoomImage(imageBytes),
				default
			);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async void ViewAnimation_FromFile_Succeeds()
	{
		var device = GetFirstDevice();

		var divoomAnimation = GetDivoomAnimation(new FileInfo("../../../Animations/ReportMagic.gif"));

		var deviceResponse = await Client
			.Bluetooth
			.ViewAnimationAsync(
				device,
				divoomAnimation,
				default);

		deviceResponse.IsOk.Should().BeTrue();
	}

	private DivoomAnimation GetDivoomAnimation(FileInfo fileInfo)
	{
		var animation = new DivoomAnimation();
		var frameTime = TimeSpan.Zero;
		using var image = Image.Load<Rgb24>(fileInfo.FullName);
		foreach (var frame in image.Frames)
		{
			var frameImageBytes = new Color[256];
			var pixelIndex = 0;
			frame.ProcessPixelRows(accessor =>
			{
				for (var y = 0; y < 16; y++)
				{
					Span<Rgb24> pixelRow = accessor.GetRowSpan(y);
					for (var x = 0; x < 16; x++)
					{
						ref Rgb24 pixel = ref pixelRow[x];

						var r = pixel.R;
						var g = pixel.G;
						var b = pixel.B;
						frameImageBytes[pixelIndex++] = Color.FromArgb(r, g, b);
					}
				}
			});

			var frameDelay = TimeSpan.FromMilliseconds(frame.Metadata.GetGifMetadata().FrameDelay * 10);

			frameTime += frameDelay;

			animation.AddFrame(new DivoomImage(frameImageBytes, frameTime));
		}

		return animation;
	}

	[Fact]
	public async void ViewImage_FromFile_Succeeds()
	{
		var imageBytes = new Color[256];
		var pixelIndex = 0;
		// Load PNG image from file using SixLabors.ImageSharp
		using (var fileImage = Image.Load<Rgb24>("../../../Images/ReportMagic.png"))
		{
			if (fileImage.Width != 16 && fileImage.Height != 16)
			{
				fileImage.Mutate(x => x.Resize(16, 16));
			}

			fileImage.ProcessPixelRows(accessor =>
			{
				for (var y = 0; y < 16; y++)
				{
					Span<Rgb24> pixelRow = accessor.GetRowSpan(y);
					for (var x = 0; x < 16; x++)
					{
						ref Rgb24 pixel = ref pixelRow[x];

						var r = pixel.R;
						var g = pixel.G;
						var b = pixel.B;
						imageBytes[pixelIndex++] = Color.FromArgb(r, g, b);
					}
				}
			});
		}

		var device = GetFirstDevice();

		var deviceResponse = await Client
			.Bluetooth
			.ViewImageAsync(
				device,
				new DivoomImage(imageBytes),
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
