using AwesomeAssertions;
using Divoom.Api.Models;
using Color = System.Drawing.Color;

namespace Divoom.Api.Test;

/// <summary>
/// <see cref="BluetoothTests"/> covering reading, setting and displaying the weather.
/// </summary>
public partial class BluetoothTests
{
	[Fact]
	public async Task GetWeather_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		var deviceResponse = await Client
			.Bluetooth
			.GetWeatherAsync(device, CancellationToken);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task SetWeatherAsync_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		var deviceResponseSet = await Client
			.Bluetooth
			.SetWeatherAsync(
				device,
				30,
				WeatherType.Clear,
				CancellationToken
			);

		deviceResponseSet.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewWeather_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);

		var deviceResponse = await Client
			.Bluetooth
			.SetWeatherAsync(
				device,
				-1,
				WeatherType.Thunderstorm,
				CancellationToken);

		deviceResponse.IsOk.Should().BeTrue();

		var deviceResponseSet = await Client
			.Bluetooth
			.ViewClockAsync(
				device,
				new ClockViewSettings
				{
					TimeType = TimeType.TwelveHours,
					ClockType = ClockType.AnalogRound,
					ShowTime = true,
					Color = Color.Blue
				},
				CancellationToken);

		deviceResponseSet
			.IsOk
			.Should()
			.BeTrue();
	}
}
