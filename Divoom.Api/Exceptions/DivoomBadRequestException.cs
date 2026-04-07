using System;

namespace Divoom.Api.Exceptions;

/// <summary>
/// Exception thrown when a Divoom device returns a bad request response
/// </summary>
/// <param name="message">The error message.</param>
public class DivoomBadRequestException(string message) : Exception(message)
{
}
