using Divoom.Api.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Interfaces;

public interface IBluetooth
{
	List<DivoomBluetoothDevice> GetDevices();

	Task<DeviceResponse> SetBrightnessAsync(
		DivoomBluetoothDevice device,
		int percent,
		CancellationToken cancellationToken);

	Task<DeviceResponse> ViewTimeAsync(
		DivoomBluetoothDevice device,
		TimeType timeType,
		ClockType clockType,
		bool showTime,
		bool showWeather,
		bool showTemperature,
		bool showCalendar,
		Color color,
		CancellationToken cancellationToken);

	Task<DeviceResponse> SetDateTimeAsync(
		DivoomBluetoothDevice device,
		DateTime dateTime,
		CancellationToken cancellationToken);

	Task<DeviceResponse> SetTemperatureAndWeatherAsync(
		DivoomBluetoothDevice device,
		int temperature,
		WeatherType thunderstorm,
		CancellationToken cancellationToken);

	Task<DeviceResponse> ViewWeatherAsync(
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

	Task<DeviceResponse> ViewLightningAsync(
		DivoomBluetoothDevice device,
		Color color,
		int brightnessPercent,
		LightningType lightningType,
		CancellationToken cancellationToken);

	Task<DeviceResponse> ViewScoreboardAsync(
		DivoomBluetoothDevice device,
		int redScore,
		int blueScore,
		CancellationToken cancellationToken);

	Task<DeviceSettings> GetSettingsAsync(
		DivoomBluetoothDevice device,
		CancellationToken cancellationToken);
}
