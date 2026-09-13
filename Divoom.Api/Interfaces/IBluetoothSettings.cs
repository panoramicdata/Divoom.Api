using Divoom.Api.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Interfaces;

/// <summary>
/// Reads and writes a device's settings: brightness, volume, mute, temperature unit,
/// date and time, and the weather it displays.
/// </summary>
public interface IBluetoothSettings
{
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
	/// Gets the mute state.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<MuteState> GetMuteStateAsync(
		DivoomBluetoothDevice device,
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
	/// Gets the weather information from the device.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponse> GetWeatherAsync(
		DivoomBluetoothDevice device,
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
	/// Sets the device brightness.
	/// </summary>
	/// <param name="device">The device.</param>
	/// <param name="percent">The brightness percentage.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<DeviceResponseSet> SetBrightnessAsync(
		DivoomBluetoothDevice device,
		int percent,
		CancellationToken cancellationToken);
}
