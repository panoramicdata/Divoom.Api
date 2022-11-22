using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public abstract class PagedReturnResponse : ReturnResponse
{
	/// <summary>
	/// The total available items.
	/// </summary>
	[DataMember(Name = "TotalNum")]
	public string TotalCount { get; set; } = string.Empty;
}