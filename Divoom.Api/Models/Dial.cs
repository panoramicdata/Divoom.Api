using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A dial
/// </summary>
public class Dial
{
	/// <summary>
	/// The clock id
	/// </summary>
	[JsonPropertyName("ClockId")]
	public int ClockId { get; set; }

	/// <summary>
	///  The dial name
	/// </summary>
	[JsonPropertyName("Name")]
	public string Name { get; set; } = string.Empty;
}