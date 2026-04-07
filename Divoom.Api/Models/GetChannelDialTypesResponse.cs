using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A response containing channel dials
/// </summary>
public class GetChannelDialsResponse : PagedReturnResponse
{
	/// <summary>
	/// The list of dials
	/// </summary>
	[JsonPropertyName("DialList")]
	public ICollection<Dial> Dials { get; set; } = null!;
}