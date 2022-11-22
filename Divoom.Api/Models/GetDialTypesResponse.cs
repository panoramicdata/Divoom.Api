using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public class GetDialTypesResponse : ReturnResponse
{
	/// <summary>
	/// The list of dial types.
	/// </summary>
	[DataMember(Name = "DialTypeList")]
	public ICollection<string> DialTypes { get; set; } = null!;
}
