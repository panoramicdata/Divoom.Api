namespace Divoom.Api.Models;

/// <summary>
/// A request to get the device's settings
/// </summary>
public class GetAllConfigRequest : BasicCommand
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GetAllConfigRequest"/> class.
	/// </summary>
	public GetAllConfigRequest() : base("Channel/GetAllConf")
	{
	}
}
