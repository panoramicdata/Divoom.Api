using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A response containing dial fonts
/// </summary>
public class GetDialFontsResponse : ReturnResponse
{
	/// <summary>
	/// The list of devices.
	/// </summary>
	[JsonPropertyName("FontList")]
	public ICollection<Font> Fonts { get; set; } = null!;
}