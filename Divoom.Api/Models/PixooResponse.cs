using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public class PixooResponse
{
	[DataMember(Name = "error_code")]
	public object ReturnCode { get; set; } = -1;
}