using Divoom.Api.Interfaces;
using Divoom.Api.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Implementations;

internal sealed class BluetoothManager(ILogger logger) : IBluetooth
{
	private readonly DeviceConnection _connection = new();

	#region Get

	public Task<List<DivoomBluetoothDevice>> GetDevicesAsync(
		CancellationToken cancellationToken)
		=> GetDevicesAsync(DiscoveryMode.PairedOnly, cancellationToken);

	public Task<List<DivoomBluetoothDevice>> GetDevicesAsync(
		DiscoveryMode discoveryMode,
		CancellationToken cancellationToken)
		=> BluetoothDeviceDiscovery.GetDevicesAsync(logger, discoveryMode, cancellationToken);

	public async Task<DeviceSettings> GetSettingsAsync(
	DivoomBluetoothDevice device,
	CancellationToken cancellationToken)
	{
		var deviceResponse = await _connection.SendCommandAsync(device, cancellationToken, (byte)Command.GetSettings);

		return new DeviceSettings(deviceResponse);
	}

	public async Task<DeviceResponse> GetWeatherAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken)
	{
		var responseSet = await _connection.SendCommandAsync(device, cancellationToken, (byte)Command.GetWeather);
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

		return await _connection.SendCommandAsync(
			device,
			cancellationToken,
			(byte)Command.SetBrightness,
			(byte)brightness);
	}

	public async Task SetMuteStateAsync(
		DivoomBluetoothDevice device,
		MuteState muteState,
		CancellationToken cancellationToken)
	{
		_ = await _connection.SendCommandAsync(
			device,
			cancellationToken,
			(byte)Command.SetMuteState,
			(byte)muteState);
	}

	public async Task SetTemperatureUnitAsync(
		DivoomBluetoothDevice device,
		TemperatureUnit temperatureUnit,
		CancellationToken cancellationToken)
	{
		_ = await _connection.SendCommandAsync(
			device,
			cancellationToken,
			(byte)Command.SetTemperatureUnit,
			(byte)temperatureUnit);
	}

	public async Task SetDateTimeAsync(
		DivoomBluetoothDevice device,
		DateTime dateTime,
		CancellationToken cancellationToken)
	{
		_ = await _connection.SendCommandAsync(
			device,
			cancellationToken,
			(byte)Command.SetDateTime,
			(byte)(dateTime.Year & 0xff),
			(byte)(dateTime.Year >> 8 & 0xff),
			(byte)dateTime.Month,
			(byte)dateTime.Day,
			(byte)dateTime.Hour,
			(byte)dateTime.Minute,
			(byte)dateTime.Second);
	}

	public async Task<DeviceResponseSet> SetWeatherAsync(
		DivoomBluetoothDevice device,
		int temperature,
		WeatherType weatherType,
		CancellationToken cancellationToken)
	{
		var temperatureByte = (byte)(temperature < 0 ? temperature + 256 : temperature);

		return await _connection.SendCommandAsync(
			device,
			cancellationToken,
			(byte)Command.SetWeather,
			temperatureByte,
			(byte)weatherType);
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

		_ = await _connection.SendCommandAsync(
			device,
			cancellationToken,
			(byte)Command.SetVolume,
			(byte)volume);
	}

	#endregion

	#region View

	public async Task<DeviceResponseSet> ViewClockAsync(
		DivoomBluetoothDevice device,
		ClockViewSettings settings,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(settings);

		if (settings.BrightnessPercent < 0 || settings.BrightnessPercent > 100)
		{
			throw new ArgumentOutOfRangeException(nameof(settings), "BrightnessPercent must be between 0 and 100.");
		}

		return await _connection.SendCommandAsync(
			device,
			cancellationToken,
			(byte)Command.SetChannel,
			(byte)Channel.Clock,
			settings.Color.R,
			settings.Color.G,
			settings.Color.B,
			(byte)settings.BrightnessPercent,
			0x64,
			settings.ShowTime ? (byte)0x01 : (byte)0x00,
			settings.ShowWeather ? (byte)0x01 : (byte)0x00,
			settings.ShowTemperature ? (byte)0x01 : (byte)0x00,
			settings.ShowCalendar ? (byte)0x01 : (byte)0x00);
	}

	public async Task<DeviceResponse> ViewClock2Async(
		DivoomBluetoothDevice device,
		ClockViewSettings settings,
		CancellationToken cancellationToken
		)
	{
		ArgumentNullException.ThrowIfNull(settings);

		var responseSet = await _connection.SendCommandAsync(
			device,
			cancellationToken,
			(byte)Command.SetChannel,
			(byte)Channel.Clock,
			(byte)settings.TimeType,
			(byte)settings.ClockType,
			(byte)(settings.ShowTime ? 1 : 0),
			(byte)(settings.ShowWeather ? 1 : 0),
			(byte)(settings.ShowTemperature ? 1 : 0),
			(byte)(settings.ShowCalendar ? 1 : 0),
			settings.Color.R,
			settings.Color.G,
			settings.Color.B);

		return responseSet.Responses.First();
	}

	public async Task<int> GetVolumeAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken)
	{
		var deviceReponseSet = await _connection.SendCommandAsync(device, cancellationToken, (byte)Command.GetVolume);

		var deviceResponse = deviceReponseSet.Responses.Single();

		return deviceResponse.Bytes[0];
	}

	public async Task<MuteState> GetMuteStateAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken)
	{
		var deviceReponseSet = await _connection.SendCommandAsync(device, cancellationToken, (byte)Command.GetMuteState);

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

		return await _connection.SendCommandAsync(
			device,
			cancellationToken,
			(byte)Command.SetChannel,
			(byte)Channel.Lighting,
			color.R,
			color.G,
			color.B,
			(byte)brightnessPercent,
			(byte)lightingPattern,
			(byte)powerStatus);
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
		return await _connection.SendCommandAsync(
			device,
			cancellationToken,
			(byte)Command.SetChannel,
			(byte)channel);
	}

	public async Task<DeviceResponseSet> ViewStopwatchAsync(
		DivoomBluetoothDevice device,
		TimeSpan timeSpan,
		CancellationToken cancellationToken)
	{
		_ = await SetBrightnessAsync(device, 100, cancellationToken);

		return await _connection.SendCommandAsync(
			device,
			cancellationToken,
			(byte)Command.SetChannel,
			0x01);
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
		return await _connection.SendCommandAsync(
			device,
			cancellationToken,
			(byte)Command.SetChannel,
			(byte)Channel.Visualisation,
			(byte)visualizationType);
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

		return await _connection.SendCommandAsync(
			device,
			cancellationToken,
			(byte)Command.SetChannel,
			(byte)Channel.Scoreboard,
			0x00,
			(byte)(redScoreUshort & 0xff),
			(byte)(redScoreUshort >> 8 & 0xff),
			(byte)(blueScoreUshort & 0xff),
			(byte)(blueScoreUshort >> 8 & 0xff));
	}

	/// <summary>
	/// Views an image
	/// </summary>
	/// <param name="device">The device</param>
	/// <param name="divoomImage">An array of 256 colors, one for each pixel starting top left, moving left to right, then top to bottom.</param>
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

		return await _connection.SendCommandAsync(device, commandBuilder, cancellationToken);
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
			var frameDataBytes = animation.GetPacket(packetIndex);

			if (frameDataBytes.Count == 0)
			{
				break;
			}

			var commandBuilder = BuildAnimationFrameCommand(animationLength, packetIndex++, frameDataBytes);

			_ = await _connection.SendCommandAsync(device, commandBuilder, cancellationToken);
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
	public Task<DeviceResponseSet> ReadResponseAsync(
		DivoomBluetoothDevice device,
		TimeSpan readDelay,
		CancellationToken cancellationToken)
		=> _connection.ReadResponseAsync(device, readDelay, cancellationToken);

	#region Private

	/// <summary>
	/// Builds the command for a single animation frame.
	/// </summary>
	private static CommandBuilder BuildAnimationFrameCommand(
		int animationLength,
		int packetIndex,
		List<byte> frameDataBytes)
	{
		var commandBuilder = new CommandBuilder();

		// HEAD
		commandBuilder.Add((byte)Command.SetAnimationFrame);

		// Animation length
		commandBuilder.Add((byte)(animationLength & 0xff));
		commandBuilder.Add((byte)(animationLength >> 8 & 0xff));

		commandBuilder.Add((byte)packetIndex);

		foreach (var frameDataByte in frameDataBytes)
		{
			commandBuilder.Add(frameDataByte);
		}

		return commandBuilder;
	}

	#endregion
}