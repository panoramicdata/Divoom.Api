using Divoom.Api.Interfaces;
using Divoom.Api.Models;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Divoom.Api.Implementations
{
	internal class BluetoothManager : IBluetooth
	{
		private readonly Dictionary<long, Stream> _bluetoothClients = new();

		public List<DivoomBluetoothDevice> GetDevices()
		{
			// Enumerate all Bluetooth devices.
			var devices = new BluetoothClient().DiscoverDevices();

			return devices
				.Where(x => x.DeviceName.Contains("TimeBox"))
				.Select(x => new DivoomBluetoothDevice(x))
				.ToList();
		}

		public void ViewTime(
			DivoomBluetoothDevice device,
			TimeType timeType,
			ClockType clockType,
			bool showTime,
			bool showWeather,
			bool showTemperature,
			bool showCalendar,
			Color color
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


			var stream = GetStream(device);
			var bytes = commandBuilder.GetBytes();
			stream.Write(bytes, 0, bytes.Length);
		}

		public void ViewTemperature(DivoomBluetoothDevice device,
			TemperatureUnit temperatureUnit)
		{
			var commandBuilder = new CommandBuilder();
			commandBuilder.Add((byte)Command.SetChannel);
			commandBuilder.Add((byte)Channel.Lightning);
			commandBuilder.Add((byte)temperatureUnit);

			var stream = GetStream(device);
			var bytes = commandBuilder.GetBytes();
			stream.Write(bytes, 0, bytes.Length);
		}

		public void SetBrightness(
			DivoomBluetoothDevice device,
			int brightness)
		{
			// Brightness should be in the range 0 to 100
			if (brightness < 0 || brightness > 100)
			{
				throw new ArgumentOutOfRangeException(nameof(brightness), "Should be in the range 0 to 100");
			}

			var commandBuilder = new CommandBuilder();
			commandBuilder.Add((byte)Command.SetBrightness);
			commandBuilder.Add((byte)brightness);
			var stream = GetStream(device);
			var bytes = commandBuilder.GetBytes();
			stream.Write(bytes, 0, bytes.Length);
		}

		public byte[] ReadBytes(DivoomBluetoothDevice device)
		{
			var stream = GetStream(device);

			// Read all available bytes from the stream
			var rawBytes = new List<byte>();
			uint length = 0;
			var byteIndex = 0;
			var nextByteIsEscaped = false;
			while (stream.CanRead)
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
						continue;
					case 2:
						length |= ((uint)((byte)(byteAsInt & 0xff))) << 8;
						continue;
					default:
						if (byteAsInt == 2)
						{
							// End of message
							return rawBytes.ToArray();
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

			throw new Exception("Could not read bytes from stream");
		}
		private Stream GetStream(DivoomBluetoothDevice device)
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

		public void SetDateTime(DivoomBluetoothDevice device, DateTime dateTime)
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

			var stream = GetStream(device);
			var bytes = commandBuilder.GetBytes();
			stream.Write(bytes, 0, bytes.Length);
		}

		public void SetTemperatureAndWeather(
			DivoomBluetoothDevice device,
			int temperature,
			WeatherType weatherType)
		{
			var commandBuilder = new CommandBuilder();
			commandBuilder.Add((byte)Command.SetTemperatureAndWeather);

			var temperatureByte = (byte)(temperature < 0 ? temperature + 256 : temperature);

			commandBuilder.Add(temperatureByte);
			commandBuilder.Add((byte)weatherType);

			var stream = GetStream(device);
			var bytes = commandBuilder.GetBytes();
			stream.Write(bytes, 0, bytes.Length);
		}

		public void ViewWeather(DivoomBluetoothDevice device)
		{
			var commandBuilder = new CommandBuilder();
			commandBuilder.Add((byte)Command.SetChannel);
			commandBuilder.Add((byte)Channel.Lightning);

			var stream = GetStream(device);
			var bytes = commandBuilder.GetBytes();
			stream.Write(bytes, 0, bytes.Length);
		}

		public void ViewCloudChannel(DivoomBluetoothDevice device)
		{
			var commandBuilder = new CommandBuilder();
			commandBuilder.Add((byte)Command.SetChannel);
			commandBuilder.Add((byte)Channel.CloudChannel);

			var stream = GetStream(device);
			var bytes = commandBuilder.GetBytes();
			stream.Write(bytes, 0, bytes.Length);
		}

		public void ViewVjEffects(DivoomBluetoothDevice device)
		{
			var commandBuilder = new CommandBuilder();
			commandBuilder.Add((byte)Command.SetChannel);
			commandBuilder.Add((byte)Channel.VjEffects);

			var stream = GetStream(device);
			var bytes = commandBuilder.GetBytes();
			stream.Write(bytes, 0, bytes.Length);
		}

		public void ViewVisualization(DivoomBluetoothDevice device)
		{
			var commandBuilder = new CommandBuilder();
			commandBuilder.Add((byte)Command.SetChannel);
			commandBuilder.Add((byte)Channel.Visualisation);

			var stream = GetStream(device);
			var bytes = commandBuilder.GetBytes();
			stream.Write(bytes, 0, bytes.Length);
		}

		public void ViewAnimation(DivoomBluetoothDevice device)
		{
			var commandBuilder = new CommandBuilder();
			commandBuilder.Add((byte)Command.SetChannel);
			commandBuilder.Add((byte)Channel.Animation);

			var stream = GetStream(device);
			var bytes = commandBuilder.GetBytes();
			stream.Write(bytes, 0, bytes.Length);
		}

		public void ViewLightning(
			DivoomBluetoothDevice device,
			Color color,
			int brightnessPercent,
			LightningType lightningType)
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

			var stream = GetStream(device);
			var bytes = commandBuilder.GetBytes();
			stream.Write(bytes, 0, bytes.Length);
		}

		public void ViewVisualization(DivoomBluetoothDevice device, VisualizationType visualizationType)
		{
			var commandBuilder = new CommandBuilder();
			commandBuilder.Add((byte)Command.SetChannel);
			commandBuilder.Add((byte)Channel.Visualisation);
			commandBuilder.Add((byte)visualizationType);

			var stream = GetStream(device);
			var bytes = commandBuilder.GetBytes();
			stream.Write(bytes, 0, bytes.Length);
		}

		public void ViewScoreboard(
			DivoomBluetoothDevice device,
			int redScore,
			int blueScore)
		{
			if (redScore < 0 || redScore > 999)
			{
				throw new ArgumentOutOfRangeException(nameof(redScore));
			}

			if (blueScore < 0 || blueScore > 999)
			{
				throw new ArgumentOutOfRangeException(nameof(blueScore));
			}

			var redScoreUInt32 = (uint)redScore;
			var blueScoreUInt32 = (uint)blueScore;

			var commandBuilder = new CommandBuilder();
			commandBuilder.Add((byte)Command.SetChannel);
			commandBuilder.Add((byte)Channel.Scoreboard);
			commandBuilder.Add(0x00);
			commandBuilder.Add((byte)(redScoreUInt32 & 0xff));
			commandBuilder.Add((byte)(redScoreUInt32 >> 8 & 0xff));
			commandBuilder.Add((byte)(blueScoreUInt32 & 0xff));
			commandBuilder.Add((byte)(blueScoreUInt32 >> 8 & 0xff));

			var stream = GetStream(device);
			var bytes = commandBuilder.GetBytes();
			stream.Write(bytes, 0, bytes.Length);
		}
	}
}
