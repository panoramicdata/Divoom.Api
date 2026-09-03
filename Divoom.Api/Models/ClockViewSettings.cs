using System.Drawing;

namespace Divoom.Api.Models;

/// <summary>
/// The settings for a clock display.
/// </summary>
public class ClockViewSettings
{
	/// <summary>
	/// The time type.
	/// </summary>
	public TimeType TimeType { get; init; } = TimeType.TwentyFourHours;

	/// <summary>
	/// The clock type.
	/// </summary>
	public ClockType ClockType { get; init; } = ClockType.FullScreen;

	/// <summary>
	/// Whether to show the time.
	/// </summary>
	public bool ShowTime { get; init; }

	/// <summary>
	/// Whether to show the weather.
	/// </summary>
	public bool ShowWeather { get; init; }

	/// <summary>
	/// Whether to show the temperature.
	/// </summary>
	public bool ShowTemperature { get; init; }

	/// <summary>
	/// Whether to show the calendar.
	/// </summary>
	public bool ShowCalendar { get; init; }

	/// <summary>
	/// The color.
	/// </summary>
	public Color Color { get; init; } = Color.White;

	/// <summary>
	/// The brightness percentage, from 0 to 100.
	/// </summary>
	public int BrightnessPercent { get; init; } = 100;
}
