using Divoom.Api.Interfaces;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A request to set the channel
/// </summary>
public class SetChannelRequest : ICommand
{
	/// <summary>
	/// The command
	/// </summary>
	[JsonPropertyName("Command")]
	public string Command => "Channel/SetIndex";

	/// <summary>
	/// The channel index
	/// </summary>
	[JsonPropertyName("SelectIndex")]
	public int Index { get; set; }
}