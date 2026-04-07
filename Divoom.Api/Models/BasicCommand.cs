using Divoom.Api.Interfaces;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A basic command
/// </summary>
/// <param name="command">The command string.</param>
public abstract class BasicCommand(string command) : ICommand
{
	/// <summary>
	/// The command string
	/// </summary>
	[JsonPropertyName("Command")]
	public string Command { get; } = command;
}