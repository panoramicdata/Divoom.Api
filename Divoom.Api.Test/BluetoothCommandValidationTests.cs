using AwesomeAssertions;
using Divoom.Api.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Color = System.Drawing.Color;

namespace Divoom.Api.Test;

/// <summary>
/// Bluetooth commands that are refused before any connection is made, so they need no device.
/// </summary>
public sealed class BluetoothCommandValidationTests : IDisposable
{
	// Never dereferenced: every case here is refused before the connection is looked up
	private static readonly DivoomBluetoothDevice NoDevice = new(null!);

	private readonly DivoomClient _client = new(new DivoomClientOptions(), NullLogger.Instance);
	private readonly DivoomClient _readOnlyClient = new(new DivoomClientOptions { IsReadOnly = true }, NullLogger.Instance);

	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	private static DivoomImage SolidImage() => new([.. Enumerable.Repeat(Color.FromArgb(0, 128, 0), 256)]);

	[Fact]
	public async Task ViewImageAsync_WhenReadOnly_IsRefused()
	{
		var act = () => _readOnlyClient.Bluetooth.Images.ViewImageAsync(NoDevice, SolidImage(), CancellationToken);

		await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*read-only*");
	}

	[Fact]
	public async Task ViewAnimationAsync_WhenReadOnly_IsRefused()
	{
		var animation = new DivoomAnimation();
		animation.AddFrame(SolidImage());

		var act = () => _readOnlyClient.Bluetooth.Images.ViewAnimationAsync(NoDevice, animation, CancellationToken);

		await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*read-only*");
	}

	[Fact]
	public async Task ViewAnimationAsync_WithNoFrames_SendsNothing()
	{
		var result = await _client.Bluetooth.Images.ViewAnimationAsync(NoDevice, new DivoomAnimation(), CancellationToken);

		result.Responses.Should().BeEmpty();
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(101)]
	public async Task ViewClockAsync_WithBrightnessOutOfRange_Throws(int brightnessPercent)
	{
		var act = () => _client.Bluetooth.View.ViewClockAsync(NoDevice, new ClockViewSettings { BrightnessPercent = brightnessPercent }, CancellationToken);

		await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
	}

	[Fact]
	public async Task ViewClockAsync_WithNoSettings_Throws()
	{
		var act = () => _client.Bluetooth.View.ViewClockAsync(NoDevice, null!, CancellationToken);

		await act.Should().ThrowAsync<ArgumentNullException>();
	}

	[Fact]
	public async Task ViewClock2Async_WithNoSettings_Throws()
	{
		var act = () => _client.Bluetooth.View.ViewClock2Async(NoDevice, null!, CancellationToken);

		await act.Should().ThrowAsync<ArgumentNullException>();
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(101)]
	public async Task ViewLightingAsync_WithBrightnessOutOfRange_Throws(int brightnessPercent)
	{
		var act = () => _client.Bluetooth.View.ViewLightingAsync(NoDevice, Color.FromArgb(255, 255, 255), brightnessPercent, default, default, CancellationToken);

		await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
	}

	[Theory]
	[InlineData(-1, 0)]
	[InlineData(1000, 0)]
	[InlineData(0, -1)]
	[InlineData(0, 1000)]
	public async Task ViewScoreboardAsync_WithScoreOutOfRange_Throws(int redScore, int blueScore)
	{
		var act = () => _client.Bluetooth.View.ViewScoreboardAsync(NoDevice, redScore, blueScore, CancellationToken);

		await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(101)]
	public async Task SetBrightnessAsync_OutOfRange_Throws(int brightness)
	{
		var act = () => _client.Bluetooth.Settings.SetBrightnessAsync(NoDevice, brightness, CancellationToken);

		await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(17)]
	public async Task SetVolumeAsync_OutOfRange_Throws(int volume)
	{
		var act = () => _client.Bluetooth.Settings.SetVolumeAsync(NoDevice, volume, CancellationToken);

		await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void ClockViewSettings_Defaults()
	{
		var settings = new ClockViewSettings();

		settings.TimeType.Should().Be(TimeType.TwentyFourHours);
		settings.ClockType.Should().Be(ClockType.FullScreen);
		settings.Color.Should().Be(Color.White);
		settings.BrightnessPercent.Should().Be(100);
		settings.ShowTime.Should().BeFalse();
		settings.ShowWeather.Should().BeFalse();
		settings.ShowTemperature.Should().BeFalse();
		settings.ShowCalendar.Should().BeFalse();
	}

	[Fact]
	public void Font_ReadsTheCloudFieldNames()
	{
		var font = JsonSerializer.Deserialize<Font>("{\"Id\":2,\"Name\":\"Pixel\",\"Width\":\"8\",\"High\":\"16\",\"Charset\":\"ABC\",\"Type\":1}")!;

		font.Id.Should().Be(2);
		font.Name.Should().Be("Pixel");
		font.Width.Should().Be("8");
		font.Height.Should().Be("16");
		font.Charset.Should().Be("ABC");
		font.Type.Should().Be(1);
	}

	public void Dispose()
	{
		_client.Dispose();
		_readOnlyClient.Dispose();
	}
}
