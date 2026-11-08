using Divoom.Api.Interfaces;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A request to sound the buzzer
/// </summary>
public class PlayBuzzerRequest : ICommand
{
	/// <summary>
	/// The command
	/// </summary>
	[JsonPropertyName("Command")]
	public string Command => "Device/PlayBuzzer";

	/// <summary>
	/// Milliseconds on in each cycle
	/// </summary>
	[JsonPropertyName("ActiveTimeInCycle")]
	public int ActiveTimeInCycleMs { get; set; } = 500;

	/// <summary>
	/// Milliseconds off in each cycle
	/// </summary>
	[JsonPropertyName("OffTimeInCycle")]
	public int OffTimeInCycleMs { get; set; } = 500;

	/// <summary>
	/// Total milliseconds to play for
	/// </summary>
	[JsonPropertyName("PlayTotalTime")]
	public int PlayTotalTimeMs { get; set; } = 1000;
}
