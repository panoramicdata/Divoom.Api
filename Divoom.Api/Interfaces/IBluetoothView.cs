using Divoom.Api.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Interfaces;

/// <summary>
/// Chooses what a device shows: the clock, a channel, lighting, a visualization,
/// a stopwatch or a scoreboard.
/// </summary>
public interface IBluetoothView
{
	/// <summary>
	/// Views a clock display.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="settings">The clock display settings.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> ViewClockAsync(
		DivoomBluetoothDevice device,
		ClockViewSettings settings,
		CancellationToken cancellationToken);

	/// <summary>
	/// Views a clock display (variant 2).
	/// </summary>
	/// <remarks>
	/// This variant sends the time and clock types to the device and ignores
	/// <see cref="ClockViewSettings.BrightnessPercent"/>.
	/// </remarks>
	/// <param name="device">The device.</param>
	/// <param name="settings">The clock display settings.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	public Task<DeviceResponse> ViewClock2Async(
		DivoomBluetoothDevice device,
		ClockViewSettings settings,
		CancellationToken cancellationToken
		);

	/// <summary>
	/// Views a channel.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="channel">The channel.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> ViewChannelAsync(
		DivoomBluetoothDevice device,
		Channel channel,
		CancellationToken cancellationToken);

	/// <summary>
	/// Views a lighting display.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="color">The color.</param>
	/// <param name="brightnessPercent">The brightness percentage.</param>
	/// <param name="lightingPattern">The lighting pattern.</param>
	/// <param name="powerStatus">The power state.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> ViewLightingAsync(
		DivoomBluetoothDevice device,
		Color color,
		int brightnessPercent,
		LightingPattern lightingPattern,
		PowerState powerStatus,
		CancellationToken cancellationToken);

	/// <summary>
	/// Views a visualization display.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="visualizationType">The visualization type.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> ViewVisualizationAsync(
		DivoomBluetoothDevice device,
		VisualizationType visualizationType,
		CancellationToken cancellationToken);

	/// <summary>
	/// Views a stopwatch.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="timeSpan">The time span.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> ViewStopwatchAsync(
		DivoomBluetoothDevice device,
		TimeSpan timeSpan,
		CancellationToken cancellationToken);

	/// <summary>
	/// Views a scoreboard.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="redScore">The red team score.</param>
	/// <param name="blueScore">The blue team score.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> ViewScoreboardAsync(
		DivoomBluetoothDevice device,
		int redScore,
		int blueScore,
		CancellationToken cancellationToken);
}
