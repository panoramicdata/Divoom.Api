using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A response containing dial types
/// </summary>
public class GetDialTypesResponse : ReturnResponse
{
	/// <summary>
	/// The list of dial types.
	/// </summary>
	[JsonPropertyName("DialTypeList")]
	public ICollection<string> DialTypes { get; set; } = null!;
}
