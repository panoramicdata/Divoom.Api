using Divoom.Api.Interfaces;
using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public abstract class BasicCommand : ICommand
{
	protected BasicCommand(string command)
	{
		Command = command;
	}

	[DataMember(Name = "Command")]
	public string Command { get; }
}