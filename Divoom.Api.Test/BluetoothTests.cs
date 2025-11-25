using AwesomeAssertions;
using Divoom.Api.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = System.Drawing.Color;
using Image = SixLabors.ImageSharp.Image;

namespace Divoom.Api.Test;

public class BluetoothTests(ITestOutputHelper testOutputHelper) : Test(testOutputHelper)
{
	[Fact]
	public void GetDivoomDevice_Succeeds()
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
			var deviceResponse = await Client
				.Bluetooth
				.SetBrightnessAsync(device, brightness, CancellationToken);

			deviceResponse.IsOk.Should().BeTrue();
		}
	}

	[Fact]
	public async Task SetVolume_To2Then3_Succeeds()
	{
		var device = GetFirstDevice();

		await Client
			.Bluetooth
			.SetVolumeAsync(device, 2, CancellationToken);

		var volumeRefetch = await Client
			.Bluetooth
			.GetVolumeAsync(device, CancellationToken);

		volumeRefetch.Should().Be(2);

		await Client
			.Bluetooth
			.SetVolumeAsync(device, 3, CancellationToken);

		volumeRefetch = await Client
			.Bluetooth
			.GetVolumeAsync(device, CancellationToken);

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
				.SetVolumeAsync(device, volume, CancellationToken);

			var volumeRefetch = await Client
				.Bluetooth
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
		var device = GetFirstDevice();
		try
		{
			await Client
				.Bluetooth
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
		var device = GetFirstDevice();
		var volume = await Client
			.Bluetooth
			.GetVolumeAsync(device, CancellationToken);

		volume.Should().BeInRange(0, 16);
	}


	[Fact]
	public async Task GetOutput_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponseSet = await Client
			.Bluetooth
			.ReadResponseAsync(device, TimeSpan.FromMilliseconds(5000), CancellationToken);
		_ = deviceResponseSet.Should().NotBeNull();
	}

	[Fact]
	public async Task GetMuteState_Succeeds()
	{
		var device = GetFirstDevice();
		var muteState = await Client
			.Bluetooth
			.GetMuteStateAsync(device, CancellationToken);
		muteState.Should().BeOneOf(MuteState.Muted, MuteState.Unmuted);
	}

	[Fact]
	public async Task SetMuteState_Succeeds()
	{
		var device = GetFirstDevice();

		await Client
			.Bluetooth
			.SetMuteStateAsync(device,
				MuteState.Muted,
				CancellationToken);
	}

	[Fact]
	public async Task SetTemperatureUnit_Succeeds()
	{
		var device = GetFirstDevice();

		await Client
			.Bluetooth
			.SetTemperatureUnitAsync(device,
				TemperatureUnit.Farenheit,
				CancellationToken);

		var refetchedTemperatureUnit = await Client
			.Bluetooth
			.GetTemperatureUnitAsync(device, CancellationToken);

		refetchedTemperatureUnit
			.Should()
			.Be(TemperatureUnit.Farenheit);

		await Client
			.Bluetooth
			.SetTemperatureUnitAsync(device,
				TemperatureUnit.Celsius,
				CancellationToken);

		refetchedTemperatureUnit = await Client
			.Bluetooth
			.GetTemperatureUnitAsync(device, CancellationToken);

		refetchedTemperatureUnit
			.Should()
			.Be(TemperatureUnit.Celsius);
	}

	[Fact]
	public async Task GetWeather_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client
			.Bluetooth
			.GetWeatherAsync(device, CancellationToken);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewTime_Succeeds()
	{
		var device = GetFirstDevice();
		_ = await Client
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
				CancellationToken);
	}

	[Fact]
	public async Task ViewClockAsync_Succeeds()
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
				CancellationToken);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewClockAsync_JustClock_Succeeds()
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
				CancellationToken);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task SetWeatherAsync_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponseSet = await Client
			.Bluetooth
			.SetWeatherAsync(
				device,
				30,
				WeatherType.Clear,
				CancellationToken
			);

		deviceResponseSet.IsOk.Should().BeTrue();
	}


	[Fact]
	public async Task ViewChannel_Succeeds()
	{
		var device = GetFirstDevice();

		foreach (var channel in Enum.GetValues<Models.Channel>())
		{
			var deviceResponseSet = await Client
				.Bluetooth
				.ViewChannelAsync(
					device,
					channel,
					CancellationToken);

			deviceResponseSet.IsOk.Should().BeTrue();

			await Task.Delay(1000, CancellationToken);
		}

		var deviceResponseSet2 = await Client
			.Bluetooth
			.ViewChannelAsync(
				device,
				Channel.Scoreboard,
				CancellationToken);

		deviceResponseSet2.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewLighting_Succeeds()
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
				CancellationToken);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewVisualization_Succeeds()
	{
		var device = GetFirstDevice();
		foreach (var visualizationType in Enum.GetValues<VisualizationType>())
		{
			var deviceResponse = await Client.Bluetooth.ViewVisualizationAsync(
				device,
				visualizationType,
				CancellationToken);

			deviceResponse.IsOk.Should().BeTrue();

			await Task.Delay(1000, CancellationToken);
		}
	}

	[Fact]
	public async Task ViewStopwatch_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client
			.Bluetooth
			.ViewStopwatchAsync(
				device,
				TimeSpan.FromMinutes(1),
				CancellationToken);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewWeather_Succeeds()
	{
		var device = GetFirstDevice();

		var deviceResponse = await Client
			.Bluetooth
			.SetWeatherAsync(
				device,
				-1,
				WeatherType.Thunderstorm,
				CancellationToken);

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
				CancellationToken);

		deviceResponseSet
			.IsOk
			.Should()
			.BeTrue();
	}

	[Fact]
	public async Task GetSettings_Succeeds()
	{
		var device = GetFirstDevice();

		_ = await Client
			.Bluetooth
			.GetSettingsAsync(device, CancellationToken);
	}

	[Fact]
	public async Task SetDateTime_Succeeds()
	{
		var device = GetFirstDevice();
		var deviceResponse = await Client
			.Bluetooth
			.SetDateTimeAsync(
				device,
				DateTime.UtcNow.AddHours(1),
				CancellationToken);

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
						blueScore, CancellationToken);

				deviceResponse.IsOk.Should().BeTrue();

				await Task.Delay(1000, CancellationToken);
			}
		}
	}

	[Fact]
	public async Task ViewImage_Constructed_Succeeds()
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
				CancellationToken
			);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewAnimation_FromFile_Succeeds()
	{
		var device = GetFirstDevice();

		var divoomAnimation = GetDivoomAnimation(new FileInfo("../../../Animations/ReportMagic.gif"));

		var deviceResponse = await Client
			.Bluetooth
			.ViewAnimationAsync(
				device,
				divoomAnimation,
				CancellationToken);

		deviceResponse.IsOk.Should().BeTrue();
	}

	private static DivoomAnimation GetDivoomAnimation(FileInfo fileInfo)
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
	public async Task ViewImage_FromFile_Succeeds()
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
				CancellationToken
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
