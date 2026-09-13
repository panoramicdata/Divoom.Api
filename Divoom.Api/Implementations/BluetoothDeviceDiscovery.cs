using Divoom.Api.Models;
using InTheHand.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Implementations;

/// <summary>
/// Finds Divoom devices over Bluetooth, whether already paired or merely nearby.
/// </summary>
/// <remarks>
/// Kept apart from <see cref="BluetoothManager"/>'s command vocabulary: finding a device
/// is a one-off concern with its own failure modes, and it needs no open connection.
/// </remarks>
internal static class BluetoothDeviceDiscovery
{
	/// <summary>
	/// The name fragments that mark a Bluetooth device as one of Divoom's.
	/// </summary>
	private static readonly string[] DivoomNameFragments = ["TimeBox", "PIXOO", "Divoom"];

	/// <summary>
	/// Finds the Divoom devices visible under the given discovery mode.
	/// </summary>
	public static async Task<List<DivoomBluetoothDevice>> GetDevicesAsync(
		ILogger logger,
		DiscoveryMode discoveryMode,
		CancellationToken cancellationToken)
	{
		if (logger.IsEnabled(LogLevel.Information))
		{
			logger.LogInformation("Starting Bluetooth device discovery with mode: {DiscoveryMode}", discoveryMode);
		}

		try
		{
			// Enumerate all Bluetooth devices.
			var bluetoothDevices = new List<BluetoothDeviceInfo>();

			// The modern InTheHand.Net.Bluetooth library discovers all paired and nearby devices
			var bluetoothClient = new BluetoothClient();

			if (discoveryMode is DiscoveryMode.All or DiscoveryMode.PairedOnly)
			{
				bluetoothDevices.AddRange(bluetoothClient.PairedDevices);
			}

			if (discoveryMode is DiscoveryMode.All or DiscoveryMode.DiscoveredOnly)
			{
				await AddDiscoveredDevicesAsync(bluetoothClient, bluetoothDevices, cancellationToken);
			}

			return FilterDivoomDevices(bluetoothDevices);
		}
		catch (Exception ex)
		{
			// Log or wrap the exception with more context
			throw new InvalidOperationException(
				"Failed to discover Bluetooth devices. Ensure Bluetooth is enabled and you have proper permissions.",
				ex);
		}
	}

	/// <summary>
	/// Discovers nearby devices and adds them to the collection.
	/// </summary>
	private static async Task AddDiscoveredDevicesAsync(
		BluetoothClient bluetoothClient,
		List<BluetoothDeviceInfo> bluetoothDevices,
		CancellationToken cancellationToken)
	{
		await foreach (var bluetoothDevice in bluetoothClient.DiscoverDevicesAsync(cancellationToken))
		{
			bluetoothDevices.Add(bluetoothDevice);
		}
	}

	/// <summary>
	/// Filters for Divoom/TimeBox devices (case-insensitive).
	/// </summary>
	/// <remarks>
	/// Common device names: "TimeBox", "TimeBox-Evo", "PIXOO64", "Pixoo", "Divoom".
	/// </remarks>
	private static List<DivoomBluetoothDevice> FilterDivoomDevices(IEnumerable<BluetoothDeviceInfo> bluetoothDevices)
		=> [.. bluetoothDevices
			.Where(x => x.DeviceName is not null
				&& DivoomNameFragments.Any(fragment =>
					x.DeviceName.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
			.Select(x => new DivoomBluetoothDevice(x))];
}
