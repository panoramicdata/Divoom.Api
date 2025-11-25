using Divoom.Api.Interfaces;
using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public abstract class BasicCommand(string command) : ICommand
{
	[DataMember(Name = "Command")]
	public string Command { get; } = command;
}