namespace Divoom.Api.Models;

/// <summary>
/// The display channel
/// </summary>
public enum Channel
{
	/// <summary>Clock</summary>
	Clock = 0x00,
	/// <summary>Lighting</summary>
	Lighting = 0x01,
	/// <summary>Cloud channel</summary>
	CloudChannel = 0x02,
	/// <summary>VJ effects</summary>
	VjEffects = 0x03,
	/// <summary>Visualisation</summary>
	Visualisation = 0x04,
	/// <summary>Animation</summary>
	Animation = 0x05,
	/// <summary>Scoreboard</summary>
	Scoreboard = 0x06,
	/// <summary>Stopwatch</summary>
	Stopwatch = 0x07,
}
