using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

public abstract class PagedReturnResponse : ReturnResponse
{
	/// <summary>
	/// The total available items.
	/// </summary>
	[JsonPropertyName("TotalNum")]
	public string TotalCount { get; set; } = string.Empty;
}