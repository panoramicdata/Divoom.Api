using System.Collections.Generic;
using System.Linq;

namespace Divoom.Api.Models;

/// <summary>
/// A set of device responses
/// </summary>
/// <param name="deviceResponses">The device responses.</param>
public class DeviceResponseSet(List<DeviceResponse> deviceResponses)
{
	/// <summary>
	/// The responses
	/// </summary>
	public List<DeviceResponse> Responses { get; } = deviceResponses;

	/// <summary>
	/// Whether all responses are OK
	/// </summary>
	public bool IsOk => Responses.All(r => r.IsOk);
}