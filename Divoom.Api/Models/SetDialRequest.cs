using Divoom.Api.Interfaces;
using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public class SetDialRequest : ICommand
{
	[DataMember(Name = "Command")]
	public string Command => "Channel/SetClockSelectId";

	[DataMember(Name = "ClockId")]
	public int DialId { get; set; }
}