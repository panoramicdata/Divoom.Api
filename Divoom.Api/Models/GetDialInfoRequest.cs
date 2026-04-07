namespace Divoom.Api.Models;

/// <summary>
/// A request to get dial info
/// </summary>
public class GetDialInfoRequest : BasicCommand
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GetDialInfoRequest"/> class.
	/// </summary>
	public GetDialInfoRequest() : base("Channel/GetClockInfo")
	{
	}
}
