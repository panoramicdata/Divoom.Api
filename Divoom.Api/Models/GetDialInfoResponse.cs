using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public class GetDialInfoResponse : PixooResponse
{
	/// <summary>
	/// The Dial Id
	/// </summary>
	[DataMember(Name = "ClockId")]
	public int DialId { get; set; }

	/// <summary>
	/// The brightness
	/// </summary>
	[DataMember(Name = "Brightness")]
	public int Brightness { get; set; }
}