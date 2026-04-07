using Divoom.Api.Interfaces;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A request to set the dial
/// </summary>
public class SetDialRequest : ICommand
{
	/// <summary>
	/// The command
	/// </summary>
	[JsonPropertyName("Command")]
	public string Command => "Channel/SetClockSelectId";

	/// <summary>
	/// The dial ID
	/// </summary>
	[JsonPropertyName("ClockId")]
	public int DialId { get; set; }
}