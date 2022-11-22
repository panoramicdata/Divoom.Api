using Divoom.Api.Models;
using FluentAssertions;
using Xunit.Abstractions;

namespace Divoom.Api.Test;

public class ChannelTests : Test
{
	public ChannelTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
	{
	}

	[Fact]
	public async void SetDialAsync_Succeeds()
	{
		var response = await Client.Channel.SetDialAsync(new Models.SetDialRequest { DialId = 0 }, default);
		response.Should().NotBeNull();
		response.ReturnCode.Should().Be(0);
	}

	[Fact]
	public async void GetDialInfoAsync_Succeeds()
	{
		var response = await Client.Channel.GetDialInfoAsync(new(), default);
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
	public async void SetAsync_Succeeds(int index)
	{
		var response = await Client.Channel.SetAsync(new SetChannelRequest
		{
			Index = index
		}, default);
		response.Should().NotBeNull();
		response.Should().BeOfType<PixooResponse>();
		response.ReturnCode.Should().Be(0);
	}
}
