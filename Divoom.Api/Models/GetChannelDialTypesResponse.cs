using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public class GetChannelDialsResponse : PagedReturnResponse
{
	/// <summary>
	/// The list of dials
	/// </summary>
	[DataMember(Name = "DialList")]
	public ICollection<Dial> Dials { get; set; } = null!;
}