using System.Runtime.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A paged request
/// </summary>
[DataContract]
public class PagedRequest
{
	/// <summary>
	/// The page number
	/// for example: 1
	/// There are 30 per page
	/// </summary>
	[DataMember(Name = "Page")]
	public int Page { get; set; } = 1;
}