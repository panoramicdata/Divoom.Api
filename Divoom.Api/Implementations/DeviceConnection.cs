using Divoom.Api.Models;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Implementations;

/// <summary>
/// Carries commands to and from a device over its serial-port Bluetooth stream.
/// </summary>
/// <remarks>
/// One stream is held open per device address and reused, because connecting is slow and
/// the device accepts only one connection at a time. This transport concern is kept apart
/// from <see cref="BluetoothManager"/>'s command vocabulary, which changes for entirely
/// different reasons.
/// </remarks>
internal sealed class DeviceConnection
{
	/// <summary>
	/// How long a device is given to answer before its pending responses are read.
	/// </summary>
	private static readonly TimeSpan DefaultReadDelay = TimeSpan.FromMilliseconds(500);

	private readonly Dictionary<ulong, NetworkStream> _streams = [];

	/// <summary>
	/// Sends a command made up of a fixed sequence of bytes, in the order given.
	/// </summary>
	public Task<DeviceResponseSet> SendCommandAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken,
		params byte[] commandBytes)
	{
		var commandBuilder = new CommandBuilder();
		foreach (var commandByte in commandBytes)
		{
			commandBuilder.Add(commandByte);
		}

		return SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	/// <summary>
	/// Sends a built command and reads whatever the device answers with.
	/// </summary>
	public async Task<DeviceResponseSet> SendCommandAsync(
		DivoomBluetoothDevice device,
		CommandBuilder commandBuilder,
		CancellationToken cancellationToken)
	{
		var stream = GetStream(device);
		var bytes = commandBuilder.GetBytes();
		stream.Write(bytes, 0, bytes.Length);

		return await ReadResponseAsync(device, DefaultReadDelay, cancellationToken);
	}

	/// <summary>
	/// Reads any pending messages from the device.
	/// </summary>
	/// <param name="device">The device</param>
	/// <param name="readDelay">The read delay</param>
	/// <param name="cancellationToken">The CancellationToken</param>
	public async Task<DeviceResponseSet> ReadResponseAsync(
		DivoomBluetoothDevice device,
		TimeSpan readDelay,
		CancellationToken cancellationToken)
	{
		var stream = GetStream(device);

		await Task.Delay(readDelay, cancellationToken);

		var responses = new List<DeviceResponse>();
		while (true)
		{
			var response = DeviceResponseReader.Read(stream);
			if (response.IsEmpty)
			{
				break;
			}

			responses.Add(response);
		}

		return new DeviceResponseSet(responses);
	}

	/// <summary>
	/// Returns the open stream for a device, connecting on first use.
	/// </summary>
	private NetworkStream GetStream(DivoomBluetoothDevice device)
	{
		if (_streams.TryGetValue(device.DeviceInfo.DeviceAddress, out var stream))
		{
			return stream;
		}

		// Verify device is reachable before connecting
		if (!device.DeviceInfo.Connected)
		{
			throw new InvalidOperationException(
				$"Device '{device.DeviceInfo.DeviceName}' is paired but not currently connected. " +
				"Ensure the device is powered on and within range.");
		}

		// Connect to the device.
		var bluetoothClient = new BluetoothClient();
		bluetoothClient.Connect(new BluetoothEndPoint(device.DeviceInfo.DeviceAddress, BluetoothService.SerialPort, 1));
		stream = bluetoothClient.GetStream();
		_streams.Add(device.DeviceInfo.DeviceAddress, stream);

		return stream;
	}
}
