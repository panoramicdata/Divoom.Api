using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A response containing LAN devices
/// </summary>
public class GetLanDevicesResponse : ReturnResponse
{
	/// <summary>
	/// The list of devices.
	/// </summary>
	[JsonPropertyName("DeviceList")]
	public ICollection<Device> Devices { get; set; } = null!;
}
