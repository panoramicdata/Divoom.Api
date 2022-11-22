using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public class GetImagesResponse : ReturnResponse
{
	/// <summary>
	/// The list of images
	/// </summary>
	[DataMember(Name = "ImgList")]
	public ICollection<Image> Images { get; set; } = null!;

	/// <summary>
	/// The device id
	/// </summary>
	[DataMember(Name = "DeviceId")]
	public int DeviceId { get; set; }
}