using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A return response
/// </summary>
public abstract class ReturnResponse
{
	/// <summary>
	/// The return code:
	/// 0 means success
	/// </summary>
	[JsonPropertyName("ReturnCode")]
	public int ReturnCode { get; set; }

	/// <summary>
	/// The return message.
	/// </summary>
	[JsonPropertyName("ReturnMessage")]
	public string ReturnMessage { get; set; } = string.Empty;
}