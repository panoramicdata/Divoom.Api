using AwesomeAssertions;
using Divoom.Api.Models;
using System.Text.Json;

namespace Divoom.Api.Test;

/// <summary>
/// Unit tests for deserializing paged responses. These need neither a device nor
/// configuration, so they run anywhere.
/// </summary>
public class PagedReturnResponseTests
{
	/// <summary>
	/// A response captured from GET https://app.divoom-gz.com/Channel/GetDialList
	/// with a body of {"DialType":"Social","Page":1}. Note that TotalNum is a JSON
	/// number, not a string.
	/// </summary>
	private const string GetDialListJson = """
		{
			"ReturnCode": 0,
			"ReturnMessage": "",
			"TotalNum": 26,
			"DialList": [
				{ "ClockId": 26, "Name": "Facebook Video" },
				{ "ClockId": 38, "Name": "YouTube Account" }
			]
		}
		""";

	[Fact]
	public void Deserialize_GetChannelDialsResponse_ReadsNumericTotalNum()
	{
		var response = JsonSerializer.Deserialize<GetChannelDialsResponse>(GetDialListJson);

		response.Should().NotBeNull();
		response!.ReturnCode.Should().Be(0);
		response.TotalCount.Should().Be(26);
		response.Dials.Should().HaveCount(2);
		response.Dials.First().ClockId.Should().Be(26);
		response.Dials.First().Name.Should().Be("Facebook Video");
	}
}
