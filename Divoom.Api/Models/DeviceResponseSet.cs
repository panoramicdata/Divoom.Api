using System.Collections.Generic;
using System.Linq;

namespace Divoom.Api.Models;

public class DeviceResponseSet(List<DeviceResponse> deviceResponses)
{
	public List<DeviceResponse> Responses { get; } = deviceResponses;

	public bool IsOk => Responses.All(r => r.IsOk);
}