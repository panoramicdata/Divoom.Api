using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A Pixoo response
/// </summary>
public class PixooResponse
{
	/// <summary>
	/// The return code
	/// </summary>
	[JsonPropertyName("error_code")]
	public object ReturnCode { get; set; } = -1;
}