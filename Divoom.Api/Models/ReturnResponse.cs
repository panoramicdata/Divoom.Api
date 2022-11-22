using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public abstract class ReturnResponse
{
	/// <summary>
	/// The return code:
	/// 0 means success
	/// </summary>
	[DataMember(Name = "ReturnCode")]
	public int ReturnCode { get; set; }

	/// <summary>
	/// The return message.
	/// </summary>
	[DataMember(Name = "ReturnMessage")]
	public string ReturnMessage { get; set; } = string.Empty;
}