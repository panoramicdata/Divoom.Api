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

	List<DivoomBluetoothDevice> GetDevices();

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

	Task<DeviceResponse> SetDateTimeAsync(
		DivoomBluetoothDevice device,
		DateTime dateTime,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> SetBrightnessAsync(
		DivoomBluetoothDevice device,
		int percent,
		CancellationToken cancellationToken);

	Task<DeviceResponse> SetWeatherAsync(
		DivoomBluetoothDevice device,
		int temperature,
		WeatherType thunderstorm,
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

	Task<DeviceResponseSet> ViewAllTheThingsAsync(
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

	Task<DeviceResponseSet> ViewLightingAsync(
		DivoomBluetoothDevice device,
		Color color,
		int brightnessPercent,
		LightingPattern lightingPattern,
		PowerState powerStatus,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ViewCloudChannelAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ViewVjEffectAsync(
		DivoomBluetoothDevice device,
		VjEffectType vjEffectType,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ViewVisualizationAsync(
		DivoomBluetoothDevice device,
		VisualizationType visualizationType,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ViewAnimationAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ViewScoreboardAsync(
		DivoomBluetoothDevice device,
		int redScore,
		int blueScore,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ViewImageAsync(
		DivoomBluetoothDevice device,
		Color[] image,
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

	Task<TemperatureUnit> GetTemperatureUnitAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ReadResponseAsync(DivoomBluetoothDevice device, TimeSpan readDelay, CancellationToken cancellationToken);

	#endregion
}