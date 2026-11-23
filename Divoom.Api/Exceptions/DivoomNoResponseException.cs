using System;

namespace Divoom.Api.Exceptions;

/// <summary>
/// Exception thrown when a Divoom device sends no reply to a command that needs one
/// </summary>
/// <param name="message">The error message.</param>
public class DivoomNoResponseException(string message) : Exception(message)
{
}