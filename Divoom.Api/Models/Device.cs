using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A device
/// </summary>
public class Device
{
	/// <summary>
	/// The device name
	/// </summary>
	[JsonPropertyName("DeviceName")]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The device ID
	/// </summary>
	[JsonPropertyName("DeviceId")]
	public int Id { get; set; }

	/// <summary>
	/// The device IP
	/// </summary>
	[JsonPropertyName("DevicePrivateIp")]
	public string PrivateIp { get; set; } = string.Empty;

	/// <summary>
	/// The device MAC
	/// </summary>
	[JsonPropertyName("DeviceMac")]
	public string Mac { get; set; } = string.Empty;
}
