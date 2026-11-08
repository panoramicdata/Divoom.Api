namespace Divoom.Api.Models;

/// <summary>
/// A request to reset the HTTP animation id
/// </summary>
public class ResetHttpGifIdRequest : BasicCommand
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ResetHttpGifIdRequest"/> class.
	/// </summary>
	public ResetHttpGifIdRequest() : base("Draw/ResetHttpGifId")
	{
	}
}
