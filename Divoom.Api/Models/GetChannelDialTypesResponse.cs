using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

public class GetChannelDialsResponse : PagedReturnResponse
{
	/// <summary>
	/// The list of dials
	/// </summary>
	[JsonPropertyName("DialList")]
	public ICollection<Dial> Dials { get; set; } = null!;
}