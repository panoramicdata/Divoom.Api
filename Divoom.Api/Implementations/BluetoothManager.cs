using Divoom.Api.Interfaces;
using Divoom.Api.Models;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Implementations;

internal sealed class BluetoothManager(ILogger logger) : IBluetooth
{
	private readonly Dictionary<ulong, NetworkStream> _bluetoothClients = [];

	#region Get

	public Task<List<DivoomBluetoothDevice>> GetDevicesAsync(
		CancellationToken cancellationToken) => GetDevicesAsync(DiscoveryMode.PairedOnly, cancellationToken);

	public async Task<List<DivoomBluetoothDevice>> GetDevicesAsync(
		DiscoveryMode discoveryMode,
		CancellationToken cancellationToken)
	{
		logger.LogInformation("Starting Bluetooth device discovery with mode: {DiscoveryMode}", discoveryMode);

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
				await foreach (var bluetoothDevice in bluetoothClient.DiscoverDevicesAsync(cancellationToken))
				{
					bluetoothDevices.Add(bluetoothDevice);
				}
			}

			// Filter for Divoom/TimeBox devices (case-insensitive)
			// Common device names: "TimeBox", "TimeBox-Evo", "PIXOO64", "Pixoo", "Divoom"
			return [.. bluetoothDevices
				.Where(x => x.DeviceName != null && (
					x.DeviceName.Contains("TimeBox", StringComparison.OrdinalIgnoreCase) ||
					x.DeviceName.Contains("PIXOO", StringComparison.OrdinalIgnoreCase) ||
					x.DeviceName.Contains("Divoom", StringComparison.OrdinalIgnoreCase)
				))
				.Select(x => new DivoomBluetoothDevice(x))];
		}
		catch (Exception ex)
		{
			// Log or wrap the exception with more context
			throw new InvalidOperationException(
				"Failed to discover Bluetooth devices. Ensure Bluetooth is enabled and you have proper permissions.",
				ex);
		}
	}

	public async Task<DeviceSettings> GetSettingsAsync(
	DivoomBluetoothDevice device,
	CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.GetSettings);

		var deviceResponse = await SendCommandAsync(device, commandBuilder, cancellationToken);

		return new DeviceSettings(deviceResponse);
	}

	public async Task<DeviceResponse> GetWeatherAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.GetWeather);
		var responseSet = await SendCommandAsync(device, commandBuilder, cancellationToken);
		return responseSet.Responses.Single();
	}

	#endregion

	#region Set

	public async Task<DeviceResponseSet> SetBrightnessAsync(
		DivoomBluetoothDevice device,
		int brightness,
		CancellationToken cancellationToken)
	{
		// Brightness should be in the range 0 to 100
		if (brightness < 0 || brightness > 100)
		{
			throw new ArgumentOutOfRangeException(nameof(brightness), "Should be in the range 0 to 100");
		}

		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetBrightness);
		commandBuilder.Add((byte)brightness);

		var responseSet = await SendCommandAsync(device, commandBuilder, cancellationToken);
		return responseSet;
	}

	public async Task SetMuteStateAsync(
		DivoomBluetoothDevice device,
		MuteState muteState,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetMuteState);
		commandBuilder.Add((byte)muteState);

		_ = await SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public async Task SetTemperatureUnitAsync(
		DivoomBluetoothDevice device,
		TemperatureUnit temperatureUnit,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetTemperatureUnit);
		commandBuilder.Add((byte)temperatureUnit);

		_ = await SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public async Task SetDateTimeAsync(
		DivoomBluetoothDevice device,
		DateTime dateTime,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetDateTime);
		commandBuilder.Add((byte)(dateTime.Year & 0xff));
		commandBuilder.Add((byte)(dateTime.Year >> 8 & 0xff));
		commandBuilder.Add((byte)dateTime.Month);
		commandBuilder.Add((byte)dateTime.Day);
		commandBuilder.Add((byte)dateTime.Hour);
		commandBuilder.Add((byte)dateTime.Minute);
		commandBuilder.Add((byte)dateTime.Second);

		_ = await SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public async Task<DeviceResponseSet> SetWeatherAsync(
		DivoomBluetoothDevice device,
		int temperature,
		WeatherType weatherType,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetWeather);

		var temperatureByte = (byte)(temperature < 0 ? temperature + 256 : temperature);

		commandBuilder.Add(temperatureByte);
		commandBuilder.Add((byte)weatherType);

		var responseSet = await SendCommandAsync(device, commandBuilder, cancellationToken);
		return responseSet;
	}

	public async Task SetVolumeAsync(
		DivoomBluetoothDevice device,
		int volume,
		CancellationToken cancellationToken)
	{
		// Volume should be in the range 0 to 16
		if (volume < 0 || volume > 16)
		{
			throw new ArgumentOutOfRangeException(nameof(volume), "Should be in the range 0 to 100");
		}

		// 3 doesn't seem to work.  Set to 2 instead
		if (volume == 3)
		{
			volume = 2;
		}

		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetVolume);
		commandBuilder.Add((byte)volume);

		_ = await SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	#endregion

	#region View

	public async Task<DeviceResponseSet> ViewClockAsync(
		DivoomBluetoothDevice device,
		TimeType timeType,
		ClockType clockType,
		bool showTime,
		bool showWeather,
		bool showTemperature,
		bool showCalendar,
		Color color,
		int brightnessPercent,
		CancellationToken cancellationToken)
	{
		if (brightnessPercent < 0 || brightnessPercent > 100)
		{
			throw new ArgumentOutOfRangeException(nameof(brightnessPercent));
		}

		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetChannel);
		commandBuilder.Add((byte)Channel.Clock);
		commandBuilder.Add(color.R);
		commandBuilder.Add(color.G);
		commandBuilder.Add(color.B);
		commandBuilder.Add((byte)brightnessPercent);
		commandBuilder.Add(0x64);
		commandBuilder.Add(showTime ? (byte)0x01 : (byte)0x00);
		commandBuilder.Add(showWeather ? (byte)0x01 : (byte)0x00);
		commandBuilder.Add(showTemperature ? (byte)0x01 : (byte)0x00);
		commandBuilder.Add(showCalendar ? (byte)0x01 : (byte)0x00);

		var responseSet = await SendCommandAsync(device, commandBuilder, cancellationToken);
		return responseSet;
	}

	public async Task<DeviceResponse> ViewClock2Async(
		DivoomBluetoothDevice device,
		TimeType timeType,
		ClockType clockType,
		bool showTime,
		bool showWeather,
		bool showTemperature,
		bool showCalendar,
		Color color,
		CancellationToken cancellationToken
		)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetChannel);
		commandBuilder.Add((byte)Channel.Clock);
		commandBuilder.Add((byte)timeType);
		commandBuilder.Add((byte)clockType);
		commandBuilder.Add((byte)(showTime ? 1 : 0));
		commandBuilder.Add((byte)(showWeather ? 1 : 0));
		commandBuilder.Add((byte)(showTemperature ? 1 : 0));
		commandBuilder.Add((byte)(showCalendar ? 1 : 0));
		commandBuilder.Add(color.R);
		commandBuilder.Add(color.G);
		commandBuilder.Add(color.B);

		var responseSet = await SendCommandAsync(device, commandBuilder, cancellationToken);
		return responseSet.Responses.First();
	}

	public async Task<int> GetVolumeAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.GetVolume);

		var deviceReponseSet = await SendCommandAsync(device, commandBuilder, cancellationToken);

		var deviceResponse = deviceReponseSet.Responses.Single();

		return deviceResponse.Bytes[0];
	}

	public async Task<MuteState> GetMuteStateAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.GetMuteState);

		var deviceReponseSet = await SendCommandAsync(device, commandBuilder, cancellationToken);

		var deviceResponse = deviceReponseSet.Responses[^1].Bytes[0];

		return (MuteState)deviceResponse;
	}

	public async Task<DeviceResponseSet> ViewLightingAsync(
		DivoomBluetoothDevice device,
		Color color,
		int brightnessPercent,
		LightingPattern lightingPattern,
		PowerState powerStatus,
		CancellationToken cancellationToken)
	{
		if (brightnessPercent < 0 || brightnessPercent > 100)
		{
			throw new ArgumentOutOfRangeException(nameof(brightnessPercent));
		}

		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetChannel);
		commandBuilder.Add((byte)Channel.Lighting);
		commandBuilder.Add(color.R);
		commandBuilder.Add(color.G);
		commandBuilder.Add(color.B);

		commandBuilder.Add((byte)brightnessPercent);

		commandBuilder.Add((byte)lightingPattern);

		commandBuilder.Add((byte)powerStatus);
		var responseSet = await SendCommandAsync(device, commandBuilder, cancellationToken);
		return responseSet;
	}

	/// <summary>
	/// Views a channel, without changing its settings
	/// </summary>
	/// <param name="device"></param>
	/// <param name="channel"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public async Task<DeviceResponseSet> ViewChannelAsync(
		DivoomBluetoothDevice device,
		Channel channel,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetChannel);
		commandBuilder.Add((byte)channel);

		var responseSet = await SendCommandAsync(device, commandBuilder, cancellationToken);
		return responseSet;
	}

	public async Task<DeviceResponseSet> ViewStopwatchAsync(
		DivoomBluetoothDevice device,
		TimeSpan timeSpan,
		CancellationToken cancellationToken)
	{
		_ = await SetBrightnessAsync(device, 100, cancellationToken);

		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetChannel);
		commandBuilder.Add(0x01);

		var responseSet = await SendCommandAsync(device, commandBuilder, cancellationToken);
		return responseSet;
	}

	/// <summary>
	/// Views a visualization that moves with the bluetooth audio signal
	/// </summary>
	/// <param name="device">The device</param>
	/// <param name="visualizationType">The visualization</param>
	/// <param name="cancellationToken">The CancellationToken</param>
	/// <returns></returns>
	public async Task<DeviceResponseSet> ViewVisualizationAsync(
		DivoomBluetoothDevice device,
		VisualizationType visualizationType,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetChannel);
		commandBuilder.Add((byte)Channel.Visualisation);
		commandBuilder.Add((byte)visualizationType);

		var responseSet = await SendCommandAsync(device, commandBuilder, cancellationToken);
		return responseSet;
	}

	/// <summary>
	/// Views a scroeboard
	/// </summary>
	/// <param name="device">The device</param>
	/// <param name="redScore">The red score (0..999)</param>
	/// <param name="blueScore">The blue score (0..999)</param>
	/// <param name="cancellationToken">The CancellationToken</param>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public async Task<DeviceResponseSet> ViewScoreboardAsync(
		DivoomBluetoothDevice device,
		int redScore,
		int blueScore,
		CancellationToken cancellationToken)
	{
		if (redScore < 0 || redScore > 999)
		{
			throw new ArgumentOutOfRangeException(nameof(redScore));
		}

		if (blueScore < 0 || blueScore > 999)
		{
			throw new ArgumentOutOfRangeException(nameof(blueScore));
		}

		var redScoreUshort = (ushort)redScore;
		var blueScoreUshort = (ushort)blueScore;

		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetChannel);
		commandBuilder.Add((byte)Channel.Scoreboard);
		commandBuilder.Add(0x00);
		commandBuilder.Add((byte)(redScoreUshort & 0xff));
		commandBuilder.Add((byte)(redScoreUshort >> 8 & 0xff));
		commandBuilder.Add((byte)(blueScoreUshort & 0xff));
		commandBuilder.Add((byte)(blueScoreUshort >> 8 & 0xff));

		var responseSet = await SendCommandAsync(device, commandBuilder, cancellationToken);
		return responseSet;
	}

	/// <summary>
	/// Views an image
	/// </summary>
	/// <param name="device">The device</param>
	/// <param name="image">An array of 256 colors, one for each pixel starting top left, moving left to right, then top to bottom.</param>
	/// <param name="cancellationToken">The CancellationToken</param>
	/// <returns></returns>
	/// <exception cref="NotSupportedException"></exception>
	public async Task<DeviceResponseSet> ViewImageAsync(
		DivoomBluetoothDevice device,
		DivoomImage divoomImage,
		CancellationToken cancellationToken)
	{
		// 44000A0A04 AA LLLL 000000 NN COLOR_DATA PIXEL_DATA
		// |<-HEAD->| |<-----------IMAGE_DATA-------------->|

		var commandBuilder = new CommandBuilder();

		// HEAD
		commandBuilder.Add((byte)Command.SetStaticImage);
		commandBuilder.Add(0x00); // Fixed
		commandBuilder.Add(0x0a); // Fixed
		commandBuilder.Add(0x0a); // Fixed
		commandBuilder.Add(0x04); // Fixed

		var imageBytes = divoomImage.GetImageBytes();
		foreach (var imageByte in imageBytes)
		{
			commandBuilder.Add(imageByte);
		}

		return await SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public async Task<DeviceResponseSet> ViewAnimationAsync(
		DivoomBluetoothDevice device,
		DivoomAnimation animation,
		CancellationToken cancellationToken)
	{
		var animationLength = animation.TotalFrameLength;

		var packetIndex = 0;
		while (true)
		{
			var commandBuilder = new CommandBuilder();

			// HEAD
			commandBuilder.Add((byte)Command.SetAnimationFrame);

			// Animation length
			commandBuilder.Add((byte)(animationLength & 0xff));
			commandBuilder.Add((byte)(animationLength >> 8 & 0xff));

			var frameDataBytes = animation.GetPacket(packetIndex);

			if (frameDataBytes.Count == 0)
			{
				break;
			}

			commandBuilder.Add((byte)packetIndex++);

			foreach (var frameDataByte in frameDataBytes)
			{
				commandBuilder.Add(frameDataByte);
			}

			_ = await SendCommandAsync(device, commandBuilder, cancellationToken);
		}

		// TODO
		return new DeviceResponseSet([]);
	}

	#endregion


	/// <summary>
	/// Reads any pending messages from the device
	/// </summary>
	/// <param name="device">The device</param>
	/// <param name="readDelay">The read delay</param>
	/// <param name="cancellationToken">The CancellationToken</param>
	/// <returns></returns>
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
			var response = ReadResponse(stream);
			if (response.IsEmpty)
			{
				break;
			}

			responses.Add(response);
		}

		return new DeviceResponseSet(responses);
	}

	#region Private

	private async Task<DeviceResponseSet> SendCommandAsync(
		DivoomBluetoothDevice device,
		CommandBuilder commandBuilder,
		CancellationToken cancellationToken)
	{
		var stream = GetStream(device);
		var bytes = commandBuilder.GetBytes();
		stream.Write(bytes, 0, bytes.Length);

		return await ReadResponseAsync(
			device,
			TimeSpan.FromMilliseconds(500),
			cancellationToken);
	}

	private static DeviceResponse ReadResponse(NetworkStream stream)
	{
		try
		{
			// Read all available bytes from the stream
			var rawBytes = new List<byte>();
			uint length = 0;
			var byteIndex = 0;
			var nextByteIsEscaped = false;
			while (stream.DataAvailable)
			{
				var byteAsInt = stream.ReadByte();

				// The first byte should be 0x01
				switch (byteIndex++)
				{
					case 0:
						if (byteAsInt != 0x01)
						{
							throw new FormatException("First byte should be 0x01");
						}

						// All is well
						continue;
					case 1:
						length = (byte)(byteAsInt & 0xff);
						rawBytes.Add((byte)byteAsInt);
						continue;
					case 2:
						length |= ((uint)((byte)(byteAsInt & 0xff))) << 8;
						rawBytes.Add((byte)byteAsInt);
						continue;
					default:
						if (byteAsInt == 2 && byteIndex == length + 4)
						{
							// Get the CRC
							var crc =
								rawBytes[^2]
								|
								(ushort)(rawBytes[^1] << 8);

							// Remove the CRC bytes
							rawBytes.RemoveRange(rawBytes.Count - 2, 2);

							// Sum the bytes
							var sum = rawBytes.Sum(x => x);
							if (sum != crc)
							{
								throw new FormatException("CRC does not match");
							}

							// Remove the Length bytes
							if (rawBytes is null)
							{
								throw new InvalidOperationException("rawBytes is null");
							}

							rawBytes.RemoveRange(0, 2);

							// Return a device response based on the raw bytes excluding length and CRC
							return new(rawBytes);
						}

						if (byteAsInt == 3)
						{
							nextByteIsEscaped = true;
							continue;
						}

						if (nextByteIsEscaped)
						{
							byteAsInt -= 3;
							nextByteIsEscaped = false;
						}

						rawBytes.Add((byte)byteAsInt);
						continue;
				}
			}

			return new([]);
		}
		catch
		{
			return new([]);
		}
	}

	private NetworkStream GetStream(DivoomBluetoothDevice device)
	{
		if (_bluetoothClients.TryGetValue(device.DeviceInfo.DeviceAddress, out var stream))
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
		_bluetoothClients.Add(device.DeviceInfo.DeviceAddress, stream);

		return stream;
	}

	#endregion
}
