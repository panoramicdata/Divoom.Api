using System.Runtime.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A device
/// </summary>
[DataContract]
public class Device
{
	/// <summary>
	/// The device name
	/// </summary>
	[DataMember(Name = "DeviceName")]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The device ID
	/// </summary>
	[DataMember(Name = "DeviceId")]
	public int Id { get; set; }

	/// <summary>
	/// The device IP
	/// </summary>
	[DataMember(Name = "DevicePrivateIp")]
	public string PrivateIp { get; set; } = string.Empty;

	/// <summary>
	/// The device MAC
	/// </summary>
	[DataMember(Name = "DeviceMac")]
	public string Mac { get; set; } = string.Empty;
}
