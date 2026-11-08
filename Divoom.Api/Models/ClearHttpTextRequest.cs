namespace Divoom.Api.Models;

/// <summary>
/// A request to remove all text sent over HTTP
/// </summary>
public class ClearHttpTextRequest : BasicCommand
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ClearHttpTextRequest"/> class.
	/// </summary>
	public ClearHttpTextRequest() : base("Draw/ClearHttpText")
	{
	}
}
