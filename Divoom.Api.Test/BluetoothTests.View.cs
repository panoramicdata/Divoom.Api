using AwesomeAssertions;
using Divoom.Api.Models;
using Color = System.Drawing.Color;

namespace Divoom.Api.Test;

/// <summary>
/// <see cref="BluetoothTests"/> covering the clock, channel, lighting, visualization, stopwatch and scoreboard views.
/// </summary>
public partial class BluetoothTests
{
	[Fact]
	public async Task ViewTime_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		_ = await Client
			.Bluetooth
			.ViewClockAsync(
				device,
				new ClockViewSettings
				{
					TimeType = TimeType.TwentyFourHours,
					ClockType = ClockType.FullScreenNegative,
					ShowWeather = true,
					Color = Color.Red
				},
				CancellationToken);
	}

	[Fact]
	public async Task ViewClockAsync_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		var deviceResponse = await Client
			.Bluetooth
			.ViewClockAsync(
				device,
				new ClockViewSettings
				{
					TimeType = TimeType.TwentyFourHours,
					ClockType = ClockType.FullScreenNegative,
					ShowTime = true,
					ShowWeather = true,
					ShowTemperature = true,
					ShowCalendar = true,
					Color = Color.Blue
				},
				CancellationToken);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewClockAsync_JustClock_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		var deviceResponse = await Client
			.Bluetooth
			.ViewClock2Async(
				device,
				new ClockViewSettings
				{
					TimeType = TimeType.TwelveHours,
					ClockType = ClockType.FullScreen,
					ShowCalendar = true,
					Color = Color.Yellow
				},
				CancellationToken);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewChannel_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);

		foreach (var channel in Enum.GetValues<Models.Channel>())
		{
			var deviceResponseSet = await Client
				.Bluetooth
				.ViewChannelAsync(
					device,
					channel,
					CancellationToken);

			deviceResponseSet.IsOk.Should().BeTrue();

			await Task.Delay(1000, CancellationToken);
		}

		var deviceResponseSet2 = await Client
			.Bluetooth
			.ViewChannelAsync(
				device,
				Channel.Scoreboard,
				CancellationToken);

		deviceResponseSet2.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewLighting_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		var deviceResponse = await Client
			.Bluetooth
			.ViewLightingAsync(
				device,
				Color.Magenta,
				100,
				LightingPattern.Custom,
				PowerState.On,
				CancellationToken);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewVisualization_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		foreach (var visualizationType in Enum.GetValues<VisualizationType>())
		{
			var deviceResponse = await Client.Bluetooth.ViewVisualizationAsync(
				device,
				visualizationType,
				CancellationToken);

			deviceResponse.IsOk.Should().BeTrue();

			await Task.Delay(1000, CancellationToken);
		}
	}

	[Fact]
	public async Task ViewStopwatch_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);
		var deviceResponse = await Client
			.Bluetooth
			.ViewStopwatchAsync(
				device,
				TimeSpan.FromMinutes(1),
				CancellationToken);

		deviceResponse.IsOk.Should().BeTrue();
	}

	[Fact]
	public async Task ViewScoreboard_Succeeds()
	{
		var device = await GetFirstDeviceAsync(CancellationToken);

		for (var redScore = 0; redScore <= 4; redScore++)
		{
			for (var blueScore = 0; blueScore <= 4; blueScore++)
			{
				var deviceResponse = await Client
					.Bluetooth
					.ViewScoreboardAsync(
						device,
						redScore,
						blueScore, CancellationToken);

				deviceResponse.IsOk.Should().BeTrue();
			}
		}
	}
}
