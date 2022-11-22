using Divoom.Api.Interfaces;
using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public class SetChannelRequest : ICommand
{
	[DataMember(Name = "Command")]
	public string Command => "Channel/SetIndex";

	[DataMember(Name = "SelectIndex")]
	public int Index { get; set; }
}