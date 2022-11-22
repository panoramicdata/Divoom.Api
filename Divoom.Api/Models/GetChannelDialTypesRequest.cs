using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public class GetChannelDialsRequest : PagedRequest
{
	/// <summary>
	/// The dial type to request
	/// </summary>
	[DataMember(Name = "DialType")]
	public string Type { get; set; } = string.Empty;
}
