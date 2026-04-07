using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

public class GetDialInfoResponse : PixooResponse
{
	/// <summary>
	/// The Dial Id
	/// </summary>
	[JsonPropertyName("ClockId")]
	public int DialId { get; set; }

	/// <summary>
	/// The brightness
	/// </summary>
	[JsonPropertyName("Brightness")]
	public int Brightness { get; set; }
}