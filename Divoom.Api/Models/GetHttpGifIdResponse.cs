using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A response containing the id of the last animation sent over HTTP
/// </summary>
public class GetHttpGifIdResponse : PixooResponse
{
	/// <summary>
	/// The animation id
	/// </summary>
	[JsonPropertyName("PicId")]
	public int PicId { get; set; }
}
