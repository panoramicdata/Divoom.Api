using AwesomeAssertions;
using Divoom.Api.Models;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using Color = System.Drawing.Color;

namespace Divoom.Api.Test;

[Collection("Bluetooth")]
public class BluetoothTests(ITestOutputHelper testOutputHelper, BluetoothFixture fixture) : IAsyncLifetime
{
	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	private DivoomClient Client => fixture.Client;

	private ILogger Logger { get; } = LoggerFactory.Create(builder => builder
		.AddProvider(new XunitLoggerProvider(testOutputHelper)))
		.CreateLogger<BluetoothTests>();

	public async ValueTask InitializeAsync() =>
		// Small delay before each test to allow device to settle
		await Task.Delay(500);

	public async ValueTask DisposeAsync()
	{
		// Small delay after each test to allow device to settle
		await Task.Delay(500);
		GC.SuppressFinalize(this);
	}

	[Fact]
	public async Task GetDivoomDevice_Succeeds()
	{
		// This test requires a physical Divoom/TimeBox/PIXOO device to be:
		// 1. Powered on
		// 2. Paired with this PC via Bluetooth
		// 3. Within range (< 10 meters)

		var device = await GetFirstDeviceAsync(CancellationToken);
		device.Should().BeOfType<DivoomBluetoothDevice>();
	}

	[Fact] // Removed Skip attribute to run diagnostics
	public async Task DiagnoseBluetooth_ListsAllDevices()
	{
		// This diagnostic test lists ALL discovered Bluetooth devices
		// to help troubleshoot why Divoom devices aren't being found

		var bluetoothClient = new InTheHand.Net.Sockets.BluetoothClient();
		var allDevices = new List<InTheHand.Net.Sockets.BluetoothDeviceInfo>();
		await foreach (var device in bluetoothClient.DiscoverDevicesAsync(CancellationToken))
		{
			allDevices.Add(device);
		}

		Logger.LogInformation("Found {AllDeviceCount} total Bluetooth devices:", allDevices.Count);

		foreach (var device in allDevices)
		{
			Logger.LogInformation(
				"  - {DeviceName} ({DeviceAddress}) - Paired: {Authenticated}, Connected: {Connected}",
				device.DeviceName,
				device.DeviceAddress,
				device.Authenticated,
				device.Connected);
		}

		allDevices.Should().NotBeNull();

		// Log which devices match our filters
		var matchingDevices = allDevices.Where(x => x.DeviceName != null && (
			x.DeviceName.Contains("TimeBox", StringComparison.OrdinalIgnoreCase) ||
			x.DeviceName.Contains("PIXOO", StringComparison.OrdinalIgnoreCase) ||
			x.DeviceName.Contains("Divoom", StringComparison.OrdinalIgnoreCase)
		)).ToList();

		Logger.LogInformation("Found {MatchingDeviceCount} Divoom/TimeBox/PIXOO devices", matchingDevices.Count);

		if (matchingDevices.Count == 0)
		{
			Logger.LogWarning("No Divoom devices found. Please ensure:");
			Logger.LogWarning("  1. Device is powered on");
			Logger.LogWarning("  2. Device is paired in Windows Bluetooth settings");
			Logger.LogWarning("  3. Device is within range");
		}
	}

	[Fact]
	public async Task SetBrightness_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
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
	public async Task SetVolume_To3_SetsTo2()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);

		await Client
			.Bluetooth
			.SetVolumeAsync(device, 7, CancellationToken);

		var volumeRefetch = await Client
			.Bluetooth
			.GetVolumeAsync(device, CancellationToken);

		volumeRefetch.Should().Be(7);

		await Client
			.Bluetooth
			.SetVolumeAsync(device, 3, CancellationToken);

		volumeRefetch = await Client
			.Bluetooth
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
		var device = await GetFirstDeviceAsync(CancellationToken);
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
		var device = await GetFirstDeviceAsync(CancellationToken);
		var volume = await Client
			.Bluetooth
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
			.GetMuteStateAsync(device, CancellationToken);
		muteState.Should().BeOneOf(MuteState.Muted, MuteState.Unmuted);
	}

	[Fact]
	public async Task SetMuteState_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);

		await Client
			.Bluetooth
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
			.SetTemperatureUnitAsync(device,
				TemperatureUnit.Farenheit,
				CancellationToken);

		await Client
			.Bluetooth
			.SetTemperatureUnitAsync(device,
				TemperatureUnit.Celsius,
				CancellationToken);
	}

	[Fact]
	public async Task GetWeather_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		var deviceResponse = await Client
			.Bluetooth
			.GetWeatherAsync(device, CancellationToken);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewTime_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
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
		var device = await GetFirstDeviceAsync(CancellationToken);
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
		var device = await GetFirstDeviceAsync(CancellationToken);
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
		var device = await GetFirstDeviceAsync(CancellationToken);
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
		var device = await GetFirstDeviceAsync(CancellationToken);

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
		var device = await GetFirstDeviceAsync(CancellationToken);
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
		var device = await GetFirstDeviceAsync(CancellationToken);
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
		var device = await GetFirstDeviceAsync(CancellationToken);
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
		var device = await GetFirstDeviceAsync(CancellationToken);

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
		var device = await GetFirstDeviceAsync(CancellationToken);

		_ = await Client
			.Bluetooth
			.GetSettingsAsync(device, CancellationToken);
	}

	[Fact]
	public async Task SetDateTime_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		await Client
			.Bluetooth
			.SetDateTimeAsync(
				device,
				DateTime.UtcNow.AddHours(1),
				CancellationToken);
	}


	[Fact]
	public async Task ViewScoreboard_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);

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

		var device = await GetFirstDeviceAsync(CancellationToken);

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
		var device = await GetFirstDeviceAsync(CancellationToken);

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

		using var stream = File.OpenRead(fileInfo.FullName);
		using var codec = SKCodec.Create(stream) ?? throw new InvalidOperationException($"Failed to load animation from {fileInfo.FullName}");
		var frameCount = codec.FrameCount;
		var info = codec.Info;

		for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
		{
			var frameInfo = codec.FrameInfo[frameIndex];
			using var bitmap = new SKBitmap(info);

			var options = new SKCodecOptions(frameIndex);
			codec.GetPixels(bitmap.Info, bitmap.GetPixels(), options);

			// Resize to 16x16 if needed
			SKBitmap resizedBitmap;
			if (bitmap.Width != 16 || bitmap.Height != 16)
			{
				resizedBitmap = bitmap.Resize(new SKImageInfo(16, 16), SKSamplingOptions.Default);
			}
			else
			{
				resizedBitmap = bitmap;
			}

			var frameImageBytes = new Color[256];
			var pixelIndex = 0;

			for (var y = 0; y < 16; y++)
			{
				for (var x = 0; x < 16; x++)
				{
					var pixel = resizedBitmap.GetPixel(x, y);
					frameImageBytes[pixelIndex++] = Color.FromArgb(pixel.Red, pixel.Green, pixel.Blue);
				}
			}

			if (resizedBitmap != bitmap)
			{
				resizedBitmap.Dispose();
			}

			var frameDelay = TimeSpan.FromMilliseconds(frameInfo.Duration);
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

		using var bitmap = SKBitmap.Decode("../../../Images/ReportMagic.png");

		// Resize to 16x16 if needed
		SKBitmap resizedBitmap;
		if (bitmap.Width != 16 || bitmap.Height != 16)
		{
			resizedBitmap = bitmap.Resize(new SKImageInfo(16, 16), SKSamplingOptions.Default);
		}
		else
		{
			resizedBitmap = bitmap;
		}

		for (var y = 0; y < 16; y++)
		{
			for (var x = 0; x < 16; x++)
			{
				var pixel = resizedBitmap.GetPixel(x, y);
				imageBytes[pixelIndex++] = Color.FromArgb(pixel.Red, pixel.Green, pixel.Blue);
			}
		}

		if (resizedBitmap != bitmap)
		{
			resizedBitmap.Dispose();
		}

		var device = await GetFirstDeviceAsync(CancellationToken);

		var deviceResponse = await Client
			.Bluetooth
			.ViewImageAsync(
				device,
				new DivoomImage(imageBytes),
				CancellationToken
			);

		deviceResponse.IsOk.Should().BeTrue();
	}

	private async Task<DivoomBluetoothDevice> GetFirstDeviceAsync(CancellationToken cancellationToken)
	{
		var devices = await Client
			.Bluetooth
			.GetDevicesAsync(cancellationToken);

		devices.Should().NotBeNull("Bluetooth discovery should not return null");

		if (devices.Count == 0)
		{
			// Provide helpful diagnostic information
			Logger.LogError("No Divoom devices found during Bluetooth discovery.");
			Logger.LogError("Troubleshooting steps:");
			Logger.LogError("  1. Ensure your Divoom device (TimeBox/PIXOO) is powered ON");
			Logger.LogError("  2. Pair the device in Windows Settings → Bluetooth & devices");
			Logger.LogError("  3. Ensure Bluetooth is enabled on your PC");
			Logger.LogError("  4. Move the device closer (within 10 meters)");
			Logger.LogError("  5. Run the DiagnoseBluetooth_ListsAllDevices test to see all discovered devices");
			Logger.LogError("");
			Logger.LogError("To run diagnostics: Remove [Skip] attribute from DiagnoseBluetooth_ListsAllDevices test");
		}

		devices.Should().BeOfType<List<DivoomBluetoothDevice>>();
		devices.Should().HaveCountGreaterThan(0, "at least one Divoom/TimeBox/PIXOO device should be discovered. See log output above for troubleshooting steps.");

		var device = devices.FirstOrDefault(d => d.DeviceInfo.Connected) ?? throw new InvalidOperationException("No connected devices found");

		Logger.LogInformation("Using device: {Device}", device);

		return device;
	}
}
