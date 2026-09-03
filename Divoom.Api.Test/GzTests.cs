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
	public Task GetImagesAsync_Succeeds()
		=> AssertImagesReturnedAsync(Client.Gz.GetImagesAsync);

	[Fact]
	public Task GetLikedImagesAsync_Succeeds()
		=> AssertImagesReturnedAsync(Client.Gz.GetLikedImagesAsync);

	// GetImagesAsync and GetLikedImagesAsync take the same request and return the same
	// response shape, so they share one request and one set of assertions.
	private async Task AssertImagesReturnedAsync(
		Func<GetImagesRequest, CancellationToken, Task<GetImagesResponse>> getImagesAsync)
	{
		var response = await getImagesAsync(new GetImagesRequest
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
