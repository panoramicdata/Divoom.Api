using Divoom.Api.Interfaces;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A request to set the brightness
/// </summary>
public class SetBrightnessRequest : ICommand
{
	/// <summary>
	/// The command
	/// </summary>
	[JsonPropertyName("Command")]
	public string Command => "Channel/SetBrightness";

	/// <summary>
	/// The brightness, 0 to 100
	/// </summary>
	[JsonPropertyName("Brightness")]
	public int Brightness { get; set; }
}
