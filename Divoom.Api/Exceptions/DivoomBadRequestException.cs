using System;

namespace Divoom.Api.Exceptions;

public class DivoomBadRequestException(string message) : Exception(message)
{
}
