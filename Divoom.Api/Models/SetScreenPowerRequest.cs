using Divoom.Api.Interfaces;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A request to switch the screen on or off
/// </summary>
public class SetScreenPowerRequest : ICommand
{
	/// <summary>
	/// The command
	/// </summary>
	[JsonPropertyName("Command")]
	public string Command => "Channel/OnOffScreen";

	/// <summary>
	/// Whether the screen should be on
	/// </summary>
	[JsonIgnore]
	public bool IsOn { get; set; }

	/// <summary>
	/// The wire value: 1 for on, 0 for off
	/// </summary>
	[JsonPropertyName("OnOff")]
	public int OnOff => IsOn ? 1 : 0;
}
