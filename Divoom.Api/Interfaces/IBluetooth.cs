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

	Task<DeviceResponse> ViewColorChangeAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);

	Task<DeviceResponse> ViewCloudChannelAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);

	Task<DeviceResponse> ViewVjEffectAsync(
		DivoomBluetoothDevice device,
		VjEffectType vjEffectType,
		CancellationToken cancellationToken);

	Task<DeviceResponse> ViewVisualizationAsync(
		DivoomBluetoothDevice device,
		VisualizationType visualizationType,
		CancellationToken cancellationToken);

	Task<DeviceResponse> ViewAnimationAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);

	Task<DeviceResponseSet> ViewScoreboardAsync(
		DivoomBluetoothDevice device,
		int redScore,
		int blueScore,
		CancellationToken cancellationToken);

	Task<DeviceResponse> ViewImageAsync(
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

	#endregion
}