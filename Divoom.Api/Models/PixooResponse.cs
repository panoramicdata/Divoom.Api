using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

public class PixooResponse
{
	[JsonPropertyName("error_code")]
	public object ReturnCode { get; set; } = -1;
}