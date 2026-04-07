using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

public class GetDialFontsResponse : ReturnResponse
{
	/// <summary>
	/// The list of devices.
	/// </summary>
	[JsonPropertyName("FontList")]
	public ICollection<Font> Fonts { get; set; } = null!;
}