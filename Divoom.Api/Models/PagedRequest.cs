using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A paged request
/// </summary>
public class PagedRequest
{
	/// <summary>
	/// The page number
	/// for example: 1
	/// There are 30 per page
	/// </summary>
	[JsonPropertyName("Page")]
	public int Page { get; set; } = 1;
}