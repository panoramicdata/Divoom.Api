using System.Collections.Generic;
using System.Linq;

namespace Divoom.Api.Models;

public class DeviceResponseSet
{
	public DeviceResponseSet(List<DeviceResponse> deviceResponses)
	{
		Responses = deviceResponses;
	}

	public List<DeviceResponse> Responses { get; }

	public bool IsOk => Responses.All(r => r.IsOk);
}