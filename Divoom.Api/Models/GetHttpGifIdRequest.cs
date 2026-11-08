namespace Divoom.Api.Models;

/// <summary>
/// A request to get the id of the last animation sent over HTTP
/// </summary>
public class GetHttpGifIdRequest : BasicCommand
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GetHttpGifIdRequest"/> class.
	/// </summary>
	public GetHttpGifIdRequest() : base("Draw/GetHttpGifId")
	{
	}
}
