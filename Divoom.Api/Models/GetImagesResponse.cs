using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A response containing images
/// </summary>
public class GetImagesResponse : ReturnResponse
{
	/// <summary>
	/// The list of images
	/// </summary>
	[JsonPropertyName("ImgList")]
	public ICollection<Image> Images { get; set; } = null!;

	/// <summary>
	/// The device id
	/// </summary>
	[JsonPropertyName("DeviceId")]
	public int DeviceId { get; set; }
}