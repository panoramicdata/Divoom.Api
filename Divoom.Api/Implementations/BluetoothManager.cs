using Divoom.Api.Interfaces;
using Divoom.Api.Models;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Implementations;

internal class BluetoothManager : IBluetooth
{
	private readonly Dictionary<long, NetworkStream> _bluetoothClients = new();

	public List<DivoomBluetoothDevice> GetDevices()
	{
		// Enumerate all Bluetooth devices.
		var devices = new BluetoothClient().DiscoverDevices();

		return devices
			.Where(x => x.DeviceName.Contains("TimeBox"))
			.Select(x => new DivoomBluetoothDevice(x))
			.ToList();
	}

	public Task<DeviceResponse> ViewTimeAsync(
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
		commandBuilder.Add((byte)Channel.Time);
		commandBuilder.Add((byte)timeType);
		commandBuilder.Add((byte)clockType);
		commandBuilder.Add((byte)(showTime ? 1 : 0));
		commandBuilder.Add((byte)(showWeather ? 1 : 0));
		commandBuilder.Add((byte)(showTemperature ? 1 : 0));
		commandBuilder.Add((byte)(showCalendar ? 1 : 0));
		commandBuilder.Add(color.R);
		commandBuilder.Add(color.G);
		commandBuilder.Add(color.B);

		return SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public Task<DeviceResponse> ViewTemperature(
		DivoomBluetoothDevice device,
		TemperatureUnit temperatureUnit,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetChannel);
		commandBuilder.Add((byte)Channel.Lightning);
		commandBuilder.Add((byte)temperatureUnit);

		return SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public Task<DeviceResponse> SetBrightnessAsync(
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

		return SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public Task<DeviceResponse> SetDateTimeAsync(
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

		return SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public Task<DeviceResponse> SetTemperatureAndWeatherAsync(
		DivoomBluetoothDevice device,
		int temperature,
		WeatherType weatherType,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetTemperatureAndWeather);

		var temperatureByte = (byte)(temperature < 0 ? temperature + 256 : temperature);

		commandBuilder.Add(temperatureByte);
		commandBuilder.Add((byte)weatherType);

		return SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public Task<DeviceResponse> ViewWeatherAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetChannel);
		commandBuilder.Add((byte)Channel.Lightning);

		return SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public Task<DeviceResponse> ViewCloudChannelAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetChannel);
		commandBuilder.Add((byte)Channel.CloudChannel);

		return SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public Task<DeviceResponse> ViewVjEffectAsync(
		DivoomBluetoothDevice device,
		VjEffectType vjEffectType,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetChannel);
		commandBuilder.Add((byte)Channel.VjEffects);
		commandBuilder.Add((byte)vjEffectType);

		return SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public Task<DeviceResponse> ViewAnimationAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetChannel);
		commandBuilder.Add((byte)Channel.Animation);

		return SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public Task<DeviceResponse> ViewLightningAsync(
		DivoomBluetoothDevice device,
		Color color,
		int brightnessPercent,
		LightningType lightningType,
		CancellationToken cancellationToken)
	{
		if (brightnessPercent < 0 || brightnessPercent > 100)
		{
			throw new ArgumentOutOfRangeException(nameof(brightnessPercent));
		}

		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetChannel);
		commandBuilder.Add((byte)Channel.Lightning);
		commandBuilder.Add(color.R);
		commandBuilder.Add(color.G);
		commandBuilder.Add(color.B);
		commandBuilder.Add((byte)brightnessPercent);
		commandBuilder.Add((byte)lightningType);
		commandBuilder.Add(0x01);
		commandBuilder.Add(0x00);
		commandBuilder.Add(0x00);
		commandBuilder.Add(0x00);

		return SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public Task<DeviceResponse> ViewVisualizationAsync(DivoomBluetoothDevice device, VisualizationType visualizationType,
		CancellationToken cancellationToken)
	{
		var commandBuilder = new CommandBuilder();
		commandBuilder.Add((byte)Command.SetChannel);
		commandBuilder.Add((byte)Channel.Visualisation);
		commandBuilder.Add((byte)visualizationType);

		return SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public Task<DeviceResponse> ViewScoreboardAsync(
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

		return SendCommandAsync(device, commandBuilder, cancellationToken);
	}

	public async Task<DeviceResponse> SendCommandAsync(
		DivoomBluetoothDevice device,
		CommandBuilder commandBuilder,
		CancellationToken cancellationToken)
	{
		var stream = GetStream(device);
		var bytes = commandBuilder.GetBytes();
		stream.Write(bytes, 0, bytes.Length);

		// Give the device a chance to respond
		await Task.Delay(500, cancellationToken);

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

		return responses.Count == 0 ? new DeviceResponse(Array.Empty<byte>()) : responses.Last();
	}

	private static DeviceResponse ReadResponse(NetworkStream stream)
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
						throw new Exception("First byte should be 0x01");
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
					if (byteAsInt == 2)
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

		return new(Array.Empty<byte>());
	}

	private NetworkStream GetStream(DivoomBluetoothDevice device)
	{
		if (_bluetoothClients.TryGetValue(device.DeviceInfo.DeviceAddress.ToInt64(), out var stream))
		{
			return stream;
		}

		// Connect to the device.
		var bluetoothClient = new BluetoothClient();
		bluetoothClient.Connect(new BluetoothEndPoint(device.DeviceInfo.DeviceAddress, BluetoothService.SerialPort, 1));
		stream = bluetoothClient.GetStream();
		_bluetoothClients.Add(device.DeviceInfo.DeviceAddress.ToInt64(), stream);

		return stream;
	}
}
