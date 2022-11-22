using System.Runtime.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A dial
/// </summary>
[DataContract]
public class Dial
{
	/// <summary>
	/// The clock id
	/// </summary>
	[DataMember(Name = "ClockId")]
	public int ClockId { get; set; }

	/// <summary>
	///  The dial name
	/// </summary>
	[DataMember(Name = "Name")]
	public string Name { get; set; } = string.Empty;
}