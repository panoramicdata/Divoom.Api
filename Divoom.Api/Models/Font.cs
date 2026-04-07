using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A font
/// </summary>
public class Font
{
	/// <summary>
	/// The font id which is used as font in Send display list
	/// </summary>
	[JsonPropertyName("Id")]
	public int Id { get; set; }

	/// <summary>
	/// the font name
	/// </summary>
	[JsonPropertyName("Name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// the font width
	/// </summary>
	[JsonPropertyName("Width")]
	public string Width { get; set; } = string.Empty;

	/// <summary>
	/// the font height
	/// </summary>
	[JsonPropertyName("High")]
	public string Height { get; set; } = string.Empty;

	/// <summary>
	/// The font include character setting
	/// </summary>
	[JsonPropertyName("Charset")]
	public string Charset { get; set; } = string.Empty;

	/// <summary>
	/// The type:
	/// 0 means will scroll if the width isn't enough
	/// 1 means does not scroll
	/// </summary>
	[JsonPropertyName("Type")]
	public int Type { get; set; }
}

