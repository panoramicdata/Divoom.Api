using Divoom.Api.Interfaces;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

public abstract class BasicCommand(string command) : ICommand
{
	[JsonPropertyName("Command")]
	public string Command { get; } = command;
}