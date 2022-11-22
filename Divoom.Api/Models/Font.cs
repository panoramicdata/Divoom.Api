using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public class Font
{
	/// <summary>
	/// The font id which is used as font in Send display list
	/// </summary>
	[DataMember(Name = "Id")]
	public int Id { get; set; }

	/// <summary>
	/// the font name
	/// </summary>
	[DataMember(Name = "Name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// the font width
	/// </summary>
	[DataMember(Name = "Width")]
	public string Width { get; set; } = string.Empty;

	/// <summary>
	/// the font height
	/// </summary>
	[DataMember(Name = "High")]
	public string Height { get; set; } = string.Empty;

	/// <summary>
	/// The font include character setting
	/// </summary>
	[DataMember(Name = "Charset")]
	public string Charset { get; set; } = string.Empty;

	/// <summary>
	/// The type:
	/// 0 means will scroll if the width isn't enough
	/// 1 means does not scroll
	/// </summary>
	[DataMember(Name = "Type")]
	public int Type { get; set; }
}

