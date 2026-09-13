using Divoom.Api.Interfaces;
using Divoom.Api.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Implementations;

/// <summary>
/// Finds Divoom devices and routes commands to them, holding one connection per device.
/// </summary>
/// <remarks>
/// The command vocabulary is grouped behind <see cref="Settings"/>, <see cref="View"/>
/// and <see cref="Images"/> rather than presented as one flat surface, so that each
/// group can be found, read and changed on its own.
/// </remarks>
internal sealed class BluetoothManager : IBluetooth
{
	private readonly ILogger _logger;
	private readonly DeviceConnection _connection = new();

	public BluetoothManager(ILogger logger)
	{
		_logger = logger;
		Settings = new BluetoothSettingsCommands(_connection);
		View = new BluetoothViewCommands(_connection, Settings);
		Images = new BluetoothImageCommands(_connection);
	}

	/// <inheritdoc />
	public IBluetoothSettings Settings { get; }

	/// <inheritdoc />
	public IBluetoothView View { get; }

	/// <inheritdoc />
	public IBluetoothImages Images { get; }

	public Task<List<DivoomBluetoothDevice>> GetDevicesAsync(
		CancellationToken cancellationToken)
		=> GetDevicesAsync(DiscoveryMode.PairedOnly, cancellationToken);

	public Task<List<DivoomBluetoothDevice>> GetDevicesAsync(
		DiscoveryMode discoveryMode,
		CancellationToken cancellationToken)
		=> BluetoothDeviceDiscovery.GetDevicesAsync(_logger, discoveryMode, cancellationToken);

	/// <summary>
	/// Reads any pending messages from the device
	/// </summary>
	/// <param name="device">The device</param>
	/// <param name="readDelay">The read delay</param>
	/// <param name="cancellationToken">The CancellationToken</param>
	/// <returns></returns>
	public Task<DeviceResponseSet> ReadResponseAsync(
		DivoomBluetoothDevice device,
		TimeSpan readDelay,
		CancellationToken cancellationToken)
		=> _connection.ReadResponseAsync(device, readDelay, cancellationToken);
}
