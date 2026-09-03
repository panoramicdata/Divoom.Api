using AwesomeAssertions;
using Divoom.Api.Models;
using Microsoft.Extensions.Logging;

namespace Divoom.Api.Test;

/// <summary>
/// Bluetooth tests covering device discovery and diagnostics.
/// </summary>
[Collection("Bluetooth")]
public class BluetoothTests(ITestOutputHelper testOutputHelper, BluetoothFixture fixture)
	: BluetoothTestBase(testOutputHelper, fixture)
{
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
}
