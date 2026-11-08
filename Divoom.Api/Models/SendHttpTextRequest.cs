using Divoom.Api.Interfaces;
using System.Text.Json.Serialization;

namespace Divoom.Api.Models;

/// <summary>
/// A request to draw text over the animation last sent with <see cref="SendHttpGifRequest"/>
/// </summary>
public class SendHttpTextRequest : ICommand
{
	/// <summary>
	/// The command
	/// </summary>
	[JsonPropertyName("Command")]
	public string Command => "Draw/SendHttpText";

	/// <summary>
	/// The text id, 0 to 19; sending the same id again replaces that text
	/// </summary>
	[JsonPropertyName("TextId")]
	public int TextId { get; set; } = 1;

	/// <summary>
	/// The left edge in pixels
	/// </summary>
	[JsonPropertyName("x")]
	public int X { get; set; }

	/// <summary>
	/// The top edge in pixels
	/// </summary>
	[JsonPropertyName("y")]
	public int Y { get; set; } = 24;

	/// <summary>
	/// The scroll direction: 0 left, 1 right
	/// </summary>
	[JsonPropertyName("dir")]
	public int Direction { get; set; }

	/// <summary>
	/// The font id
	/// </summary>
	[JsonPropertyName("font")]
	public int Font { get; set; } = 4;

	/// <summary>
	/// The text box width in pixels; 56 is verified on a Pixoo 64, where text sent at 64 did not appear
	/// </summary>
	[JsonPropertyName("TextWidth")]
	public int TextWidth { get; set; } = 56;

	/// <summary>
	/// Milliseconds per scroll step
	/// </summary>
	[JsonPropertyName("speed")]
	public int SpeedMs { get; set; } = 10;

	/// <summary>
	/// The text
	/// </summary>
	[JsonPropertyName("TextString")]
	public string Text { get; set; } = string.Empty;

	/// <summary>
	/// The colour, as #RRGGBB
	/// </summary>
	[JsonPropertyName("color")]
	public string Color { get; set; } = "#FFFFFF";

	/// <summary>
	/// The alignment: 1 left, 2 centre, 3 right
	/// </summary>
	[JsonPropertyName("align")]
	public int Align { get; set; } = 1;
}
