using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public class GetDialFontsResponse : ReturnResponse
{
	/// <summary>
	/// The list of devices.
	/// </summary>
	[DataMember(Name = "FontList")]
	public ICollection<Font> Fonts { get; set; } = null!;
}