using AwesomeAssertions;
using Divoom.Api.Models;

namespace Divoom.Api.Test;

[Collection("Network API Tests")]
public class GzTests(ITestOutputHelper testOutputHelper) : Test(testOutputHelper)
{
	[Fact]
	public async Task GetDialTypesAsync_Succeeds()
	{
		var response = await Client.Gz.GetDialTypesAsync(CancellationToken);
		response.Should().NotBeNull();
		response.ReturnCode.Should().Be(0);
		response.DialTypes.Should().NotBeEmpty();
	}

	[Fact]
	public async Task GetLanDevicesAsync_Succeeds()
	{
		var response = await Client.Gz.GetLanDevicesAsync(CancellationToken);
		response.Should().NotBeNull();
		response.ReturnCode.Should().Be(0);
		response.Devices.Should().NotBeEmpty();
	}

	[Fact]
	public async Task GetChannelDialsAsync_Succeeds()
	{
		var response = await Client.Gz.GetChannelDialsAsync(new GetChannelDialsRequest
		{
			Type = "Social",
			Page = 1
		}, CancellationToken);
		response.Should().NotBeNull();
		response.ReturnCode.Should().Be(0);
		response.Dials.Should().NotBeEmpty();
	}

	[Fact]
	public async Task GetDialFontsAsync_Succeeds()
	{
		var response = await Client.Gz.GetDialFontsAsync(CancellationToken);
		response.Should().NotBeNull();
		response.ReturnCode.Should().Be(0);
		response.Fonts.Should().NotBeEmpty();
	}

	[Fact]
	public async Task GetImagesAsync_Succeeds()
	{
		var response = await Client.Gz.GetImagesAsync(new GetImagesRequest
		{
			DeviceId = Client.Options.DeviceId,
			DeviceMac = Client.Options.DeviceMac
		}, CancellationToken);
		response.Should().NotBeNull();
		response.ReturnCode.Should().Be(0);
		response.DeviceId.Should().NotBe(0);
		response.Images.Should().NotBeNull();
	}

	[Fact]
	public async Task GetLikedImagesAsync_Succeeds()
	{
		var response = await Client.Gz.GetLikedImagesAsync(new GetImagesRequest
		{
			DeviceId = Client.Options.DeviceId,
			DeviceMac = Client.Options.DeviceMac
		}, CancellationToken);
		response.Should().NotBeNull();
		response.ReturnCode.Should().Be(0);
		response.DeviceId.Should().NotBe(0);
		response.Images.Should().NotBeNull();
	}
}
