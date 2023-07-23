using Divoom.Api.Models;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Divoom.Api.Interfaces
{
	public interface IBluetooth
	{
		List<DivoomBluetoothDevice> GetDevices();

		byte[] ReadBytes(
			DivoomBluetoothDevice device);

		void SetBrightness(
			DivoomBluetoothDevice device,
			int percent);

		void ViewTime(
			DivoomBluetoothDevice device,
			TimeType timeType,
			ClockType clockType,
			bool showTime,
			bool showWeather,
			bool showTemperature,
			bool showCalendar,
			Color color);

		void SetDateTime(
			DivoomBluetoothDevice device,
			DateTime dateTime);

		void SetTemperatureAndWeather(
			DivoomBluetoothDevice device,
			int temperature,
			WeatherType thunderstorm);

		void ViewWeather(DivoomBluetoothDevice device);

		void ViewCloudChannel(DivoomBluetoothDevice device);

		void ViewVjEffects(DivoomBluetoothDevice device);

		void ViewVisualization(
			DivoomBluetoothDevice device,
			VisualizationType visualizationType);

		void ViewAnimation(DivoomBluetoothDevice device);

		void ViewLightning(
			DivoomBluetoothDevice device,
			Color color,
			int brightnessPercent,
			LightningType lightningType);

		void ViewScoreboard(
			DivoomBluetoothDevice device,
			int redScore,
			int blueScore);
	}
}
