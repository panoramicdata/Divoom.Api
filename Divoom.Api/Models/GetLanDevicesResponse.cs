using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public class GetLanDevicesResponse : ReturnResponse
{
	/// <summary>
	/// The list of devices.
	/// </summary>
	[DataMember(Name = "DeviceList")]
	public ICollection<Device> Devices { get; set; } = null!;
}
