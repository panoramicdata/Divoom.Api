using Divoom.Api.Interfaces;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

public class SetDialRequest : ICommand
{
	[JsonPropertyName("Command")]
	public string Command => "Channel/SetClockSelectId";

	[JsonPropertyName("ClockId")]
	public int DialId { get; set; }
}