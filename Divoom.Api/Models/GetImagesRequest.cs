using System.Runtime.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A get images request
/// </summary>
[DataContract]
public class GetImagesRequest : PagedRequest
{
	/// <summary>
	/// The device id of a local devices
	/// </summary>
	[DataMember(Name = "DeviceId")]
	public int DeviceId { get; set; }

	/// <summary>
	/// The device mac of a local device
	/// </summary>
	[DataMember(Name = "DeviceMac")]
	public string DeviceMac { get; set; } = string.Empty;
}