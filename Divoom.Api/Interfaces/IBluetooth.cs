using Divoom.Api.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Interfaces;

/// <summary>
/// Bluetooth interface for communicating with Divoom devices
/// </summary>
public interface IBluetooth
{
	#region Get

	/// <summary>
	/// Gets available Bluetooth devices.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<List<DivoomBluetoothDevice>> GetDevicesAsync(
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the device settings.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceSettings> GetSettingsAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the device volume.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<int> GetVolumeAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the weather information from the device.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponse> GetWeatherAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets the mute state.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<MuteState> GetMuteStateAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);

	#endregion

	#region Set

	/// <summary>
	/// Sets the device date and time.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="dateTime">The date and time to set.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task SetDateTimeAsync(
		DivoomBluetoothDevice device,
		DateTime dateTime,
		CancellationToken cancellationToken);

	/// <summary>
	/// Sets the device brightness.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="percent">The brightness percentage.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> SetBrightnessAsync(
		DivoomBluetoothDevice device,
		int percent,
		CancellationToken cancellationToken);

	/// <summary>
	/// Sets the weather display.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="temperature">The temperature value.</param>
	/// <param name="weatherType">The weather type.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> SetWeatherAsync(
		DivoomBluetoothDevice device,
		int temperature,
		WeatherType weatherType,
		CancellationToken cancellationToken);

	/// <summary>
	/// Sets the device volume.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="volume">The volume level.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task SetVolumeAsync(
		DivoomBluetoothDevice device,
		int volume,
		CancellationToken cancellationToken);

	/// <summary>
	/// Sets the mute state.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="muteState">The mute state.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task SetMuteStateAsync(
		DivoomBluetoothDevice device,
		MuteState muteState,
		CancellationToken cancellationToken);

	#endregion

	#region View

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
	/// Views an animation.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="divoomAnimation">The animation.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> ViewAnimationAsync(
		DivoomBluetoothDevice device,
		DivoomAnimation divoomAnimation,
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

	/// <summary>
	/// Views a static image.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="image">The image to display.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> ViewImageAsync(
		DivoomBluetoothDevice device,
		DivoomImage image,
		CancellationToken cancellationToken);

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
	/// Sets the temperature unit.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="temperatureUnit">The temperature unit.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task SetTemperatureUnitAsync(
		DivoomBluetoothDevice device,
		TemperatureUnit temperatureUnit,
		CancellationToken cancellationToken);

	/// <summary>
	/// Reads a response from the device.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="readDelay">The read delay.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> ReadResponseAsync(DivoomBluetoothDevice device, TimeSpan readDelay, CancellationToken cancellationToken);

	#endregion
}