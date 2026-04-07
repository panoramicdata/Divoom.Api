using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A get images request
/// </summary>
public class GetImagesRequest : PagedRequest
{
	/// <summary>
	/// The device id of a local devices
	/// </summary>
	[JsonPropertyName("DeviceId")]
	public int DeviceId { get; set; }

	/// <summary>
	/// The device mac of a local device
	/// </summary>
	[JsonPropertyName("DeviceMac")]
	public string DeviceMac { get; set; } = string.Empty;
}