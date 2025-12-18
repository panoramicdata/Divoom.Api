using AwesomeAssertions;
using Divoom.Api.Models;

namespace Divoom.Api.Test;

[Collection("Network API Tests")]
public class ChannelTests(ITestOutputHelper testOutputHelper) : Test(testOutputHelper)
{
	[Fact]
	public async Task SetDialAsync_Succeeds()
	{
		var response = await Client.Channel.SetDialAsync(new Models.SetDialRequest { DialId = 0 }, CancellationToken);
		response.Should().NotBeNull();
		response.ReturnCode.Should().Be(0);
	}

	[Fact]
	public async Task GetDialInfoAsync_Succeeds()
	{
		var response = await Client.Channel.GetDialInfoAsync(new(), CancellationToken);
		response.Should().NotBeNull();
		response.Should().BeOfType<GetDialInfoResponse>();
		response.ReturnCode.Should().Be(0);
		response.Brightness.Should().BeInRange(0, 100);
		response.DialId.Should().BeGreaterThanOrEqualTo(0);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	public async Task SetAsync_Succeeds(int index)
	{
		var response = await Client.Channel.SetAsync(new SetChannelRequest
		{
			Index = index
		}, CancellationToken);
		response.Should().NotBeNull();
		response.Should().BeOfType<PixooResponse>();
		response.ReturnCode.Should().Be(0);
	}
}
