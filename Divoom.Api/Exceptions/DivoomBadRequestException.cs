using System;

namespace Divoom.Api.Exceptions;

public class DivoomBadRequestException : Exception
{
	public DivoomBadRequestException(string message) : base(message)
	{
	}
}
