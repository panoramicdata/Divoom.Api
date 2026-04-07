using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A request to get channel dials
/// </summary>
public class GetChannelDialsRequest : PagedRequest
{
	/// <summary>
	/// The dial type to request
	/// </summary>
	[JsonPropertyName("DialType")]
	public string Type { get; set; } = string.Empty;
}
