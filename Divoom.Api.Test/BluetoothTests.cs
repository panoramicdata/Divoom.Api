using AwesomeAssertions;
using Divoom.Api.Models;
using Microsoft.Extensions.Logging;

namespace Divoom.Api.Test;

[Collection("Bluetooth")]
public partial class BluetoothTests(ITestOutputHelper testOutputHelper, BluetoothFixture fixture) : IAsyncLifetime
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

		if (Logger.IsEnabled(LogLevel.Information))
		{
			Logger.LogInformation("Found {AllDeviceCount} total Bluetooth devices:", allDevices.Count);
		}

		foreach (var device in allDevices)
		{
			if (Logger.IsEnabled(LogLevel.Information))
			{
				Logger.LogInformation(
					"  - {DeviceName} ({DeviceAddress}) - Paired: {Authenticated}, Connected: {Connected}",
					device.DeviceName,
					device.DeviceAddress,
					device.Authenticated,
					device.Connected);
			}
		}

		allDevices.Should().NotBeNull();

		// Log which devices match our filters
		var matchingDevices = allDevices.Where(x => x.DeviceName != null && (
			x.DeviceName.Contains("TimeBox", StringComparison.OrdinalIgnoreCase) ||
			x.DeviceName.Contains("PIXOO", StringComparison.OrdinalIgnoreCase) ||
			x.DeviceName.Contains("Divoom", StringComparison.OrdinalIgnoreCase)
		)).ToList();

		if (Logger.IsEnabled(LogLevel.Information))
		{
			Logger.LogInformation("Found {MatchingDeviceCount} Divoom/TimeBox/PIXOO devices", matchingDevices.Count);
		}

		if (matchingDevices.Count == 0)
		{
			Logger.LogWarning("No Divoom devices found. Please ensure:");
			Logger.LogWarning("  1. Device is powered on");
			Logger.LogWarning("  2. Device is paired in Windows Bluetooth settings");
			Logger.LogWarning("  3. Device is within range");
		}
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

		if (Logger.IsEnabled(LogLevel.Information))
		{
			Logger.LogInformation("Using device: {Device}", device);
		}

		return device;
	}
}
