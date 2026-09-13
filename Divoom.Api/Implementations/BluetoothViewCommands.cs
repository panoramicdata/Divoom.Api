using Divoom.Api.Interfaces;
using Divoom.Api.Models;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Implementations;

/// <summary>
/// Chooses what a device shows.
/// </summary>
internal sealed class BluetoothViewCommands(DeviceConnection connection, IBluetoothSettings settings) : IBluetoothView
{
	private readonly DeviceConnection _connection = connection;

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
		_ = await settings.SetBrightnessAsync(device, 100, cancellationToken);

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
}
