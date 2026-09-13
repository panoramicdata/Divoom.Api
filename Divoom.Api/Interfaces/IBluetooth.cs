using Divoom.Api.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Interfaces;

/// <summary>
/// Finds Divoom devices over Bluetooth and gives access to what they can be told to do.
/// </summary>
public interface IBluetooth
{
	/// <summary>
	/// Gets available Bluetooth devices.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<List<DivoomBluetoothDevice>> GetDevicesAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Reads a response from the device.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="readDelay">The read delay.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> ReadResponseAsync(DivoomBluetoothDevice device, TimeSpan readDelay, CancellationToken cancellationToken);

	/// <summary>
	/// The device's settings: brightness, volume, mute, temperature unit, date and time,
	/// and the weather it displays.
	/// </summary>
	IBluetoothSettings Settings { get; }

	/// <summary>
	/// What the device shows: the clock, a channel, lighting, a visualization, a
	/// stopwatch or a scoreboard.
	/// </summary>
	IBluetoothView View { get; }

	/// <summary>
	/// Images and animations sent to the device for display.
	/// </summary>
	IBluetoothImages Images { get; }
}
