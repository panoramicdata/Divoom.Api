using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A response containing the device's clock
/// </summary>
public class GetDeviceTimeResponse : PixooResponse
{
	/// <summary>
	/// The UTC time as Unix seconds
	/// </summary>
	[JsonPropertyName("UTCTime")]
	public long UtcTime { get; set; }

	/// <summary>
	/// The local time, formatted "yyyy-MM-dd HH:mm:ss"
	/// </summary>
	[JsonPropertyName("LocalTime")]
	public string LocalTime { get; set; } = string.Empty;
}
