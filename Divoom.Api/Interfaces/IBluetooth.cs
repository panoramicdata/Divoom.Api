using Divoom.Api.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Interfaces;

public interface IBluetooth
{
	#region Get

	Task<List<DivoomBluetoothDevice>> GetDevicesAsync(
		CancellationToken cancellationToken);

	Task<DeviceSettings> GetSettingsAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);

	Task<int> GetVolumeAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);

	Task<DeviceResponse> GetWeatherAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);

	Task<MuteState> GetMuteStateAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);

	#endregion

	#region Set

	Task SetDateTimeAsync(
		DivoomBluetoothDevice device,
		DateTime dateTime,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> SetBrightnessAsync(
		DivoomBluetoothDevice device,
		int percent,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> SetWeatherAsync(
		DivoomBluetoothDevice device,
		int temperature,
		WeatherType weatherType,
		CancellationToken cancellationToken);

	Task SetVolumeAsync(
		DivoomBluetoothDevice device,
		int volume,
		CancellationToken cancellationToken);

	Task SetMuteStateAsync(
		DivoomBluetoothDevice device,
		MuteState muteState,
		CancellationToken cancellationToken);

	#endregion

	#region View

	Task<DeviceResponseSet> ViewClockAsync(
		DivoomBluetoothDevice device,
		TimeType timeType,
		ClockType clockType,
		bool showTime,
		bool showWeather,
		bool showTemperature,
		bool showCalendar,
		Color color,
		int brightnessPercent,
		CancellationToken cancellationToken);

	public Task<DeviceResponse> ViewClock2Async(
		DivoomBluetoothDevice device,
		TimeType timeType,
		ClockType clockType,
		bool showTime,
		bool showWeather,
		bool showTemperature,
		bool showCalendar,
		Color color,
		CancellationToken cancellationToken
		);

	Task<DeviceResponseSet> ViewLightingAsync(
		DivoomBluetoothDevice device,
		Color color,
		int brightnessPercent,
		LightingPattern lightingPattern,
		PowerState powerStatus,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ViewVisualizationAsync(
		DivoomBluetoothDevice device,
		VisualizationType visualizationType,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ViewAnimationAsync(
		DivoomBluetoothDevice device,
		DivoomAnimation divoomAnimation,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ViewScoreboardAsync(
		DivoomBluetoothDevice device,
		int redScore,
		int blueScore,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ViewImageAsync(
		DivoomBluetoothDevice device,
		DivoomImage image,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ViewChannelAsync(
		DivoomBluetoothDevice device,
		Channel channel,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ViewStopwatchAsync(
		DivoomBluetoothDevice device,
		TimeSpan timeSpan,
		CancellationToken cancellationToken);

	Task SetTemperatureUnitAsync(
		DivoomBluetoothDevice device,
		TemperatureUnit temperatureUnit,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ReadResponseAsync(DivoomBluetoothDevice device, TimeSpan readDelay, CancellationToken cancellationToken);

	#endregion
}