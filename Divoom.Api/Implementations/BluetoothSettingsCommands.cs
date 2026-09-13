using Divoom.Api.Interfaces;
using Divoom.Api.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Implementations;

/// <summary>
/// Reads and writes a device's settings.
/// </summary>
internal sealed class BluetoothSettingsCommands(DeviceConnection connection) : IBluetoothSettings
{
	private readonly DeviceConnection _connection = connection;

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
}
