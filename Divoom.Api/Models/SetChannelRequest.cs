using Divoom.Api.Interfaces;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

public class SetChannelRequest : ICommand
{
	[JsonPropertyName("Command")]
	public string Command => "Channel/SetIndex";

	[JsonPropertyName("SelectIndex")]
	public int Index { get; set; }
}