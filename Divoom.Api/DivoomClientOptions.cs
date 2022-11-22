using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace Divoom.Api;

public class DivoomClientOptions
{
	/// <summary>
	/// The id of a local device
	/// </summary>
	public int DeviceId { get; set; }

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
	public Action<Type, JsonSerializationException, string>? JsonMissingMemberAction { get; set; }

	public double HttpClientTimeoutSeconds { get; set; } = 30;

	public int HttpMaxAttemptCount { get; set; } = 2;
}