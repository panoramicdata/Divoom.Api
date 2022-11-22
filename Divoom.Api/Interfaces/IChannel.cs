using Divoom.Api.Models;
using Refit;
using System.Threading;
using System.Threading.Tasks;

namespace Divoom.Api.Interfaces;

/// <summary>
/// All calls that are returned from https://app.divoom-gz.com/ are served by this interface.
/// </summary>
public interface IChannel
{
	/// <summary>
	/// Gets a list of supported dial types.
	/// </summary>
	[Post("/post")]
	Task<PixooResponse> SetDialAsync(
		[Body] SetDialRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a list of supported dial types.
	/// </summary>
	[Post("/post")]
	Task<GetDialInfoResponse> GetDialInfoAsync(
		[Body] GetDialInfoRequest request,
		CancellationToken cancellationToken);

	/// <summary>
	/// Gets a list of supported dial types.
	/// </summary>
	[Post("/post")]
	Task<PixooResponse> SetAsync(
		[Body] SetChannelRequest request,
		CancellationToken cancellationToken);
}
