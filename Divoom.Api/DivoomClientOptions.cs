using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace Divoom.Api;

/// <summary>
/// Options for the Divoom client
/// </summary>
public class DivoomClientOptions
{
	/// <summary>
	/// The id of a local device
	/// </summary>
	public int DeviceId { get; set; }

	/// <summary>
	/// The IP address of a local device
	/// </summary>
	public string DeviceIp { get; set; } = string.Empty;

	/// <summary>
	/// The mac of a local device in the form 01234567890A
	/// </summary>
	public string DeviceMac { get; set; } = string.Empty;


	/// <summary>
	/// How to handle missing members
	/// </summary>
	public JsonMissingMemberHandling JsonMissingMemberHandling { get; set; } = JsonMissingMemberHandling.Ignore;

	/// <summary>
	/// The LogLevel at which response JSON will be logged when missing members are encountered. Defaults to None.
	/// </summary>
	public LogLevel JsonMissingMemberResponseLogLevel { get; set; } = LogLevel.None;

	/// <summary>
	/// This gets called when JsonMissingMemberHandling is not Ignore and a missing member occurs
	/// </summary>
	public Action<Type, JsonException, string>? JsonMissingMemberAction { get; set; }

	/// <summary>
	/// The HTTP client timeout in seconds
	/// </summary>
	public double HttpClientTimeoutSeconds { get; set; } = 30;

	/// <summary>
	/// The maximum number of HTTP retry attempts
	/// </summary>
	public int HttpMaxAttemptCount { get; set; } = 2;
}