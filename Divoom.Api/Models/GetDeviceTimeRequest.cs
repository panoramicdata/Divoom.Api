namespace Divoom.Api.Models;

/// <summary>
/// A request to get the device's clock
/// </summary>
public class GetDeviceTimeRequest : BasicCommand
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GetDeviceTimeRequest"/> class.
	/// </summary>
	public GetDeviceTimeRequest() : base("Device/GetDeviceTime")
	{
	}
}
