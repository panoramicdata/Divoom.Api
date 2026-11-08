using AwesomeAssertions;
using Divoom.Api.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Divoom.Api.Test;

public class CustomJsonContentSerializerTests
{
	private const string FontWithExtraMember = "{\"Id\":3,\"Name\":\"Small\",\"Width\":\"8\",\"High\":\"16\",\"Charset\":\"abc\",\"Type\":1,\"Extra\":true}";

	private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	private static CustomJsonContentSerializer Create(
		JsonMissingMemberHandling handling,
		RecordingLogger logger,
		LogLevel responseLogLevel = LogLevel.None,
		Action<Type, JsonException, string>? action = null)
		=> new(
			new DivoomClientOptions
			{
				JsonMissingMemberHandling = handling,
				JsonMissingMemberResponseLogLevel = responseLogLevel,
				JsonMissingMemberAction = action
			},
			logger);

	[Fact]
	public async Task Ignore_SkipsUnmappedMembers()
	{
		var logger = new RecordingLogger();
		var serializer = Create(JsonMissingMemberHandling.Ignore, logger);
		using var content = new StringContent(FontWithExtraMember);

		var font = await serializer.FromHttpContentAsync<Font>(content, CancellationToken);

		font!.Id.Should().Be(3);
		font.Height.Should().Be("16");
		logger.Entries.Should().BeEmpty();
	}

	[Fact]
	public async Task ThrowOnError_RethrowsAndReportsTheMissingMember()
	{
		var logger = new RecordingLogger();
		(Type Type, JsonException Exception, string Json)? reported = null;
		var serializer = Create(JsonMissingMemberHandling.ThrowOnError, logger, LogLevel.Information, (t, e, j) => reported = (t, e, j));
		using var content = new StringContent(FontWithExtraMember);

		var act = () => serializer.FromHttpContentAsync<Font>(content, CancellationToken);

		var thrown = (await act.Should().ThrowAsync<JsonException>()).Which;
		reported.Should().NotBeNull();
		reported!.Value.Type.Should().Be<Font>();
		reported.Value.Exception.Should().BeSameAs(thrown);
		reported.Value.Json.Should().Be(FontWithExtraMember);
		logger.MessagesAt(LogLevel.Information).Should().ContainSingle()
			.Which.Should().Be("Missing Member Response JSON:\n" + FontWithExtraMember);
	}

	[Fact]
	public async Task ThrowOnError_WithResponseLoggingOff_LogsNothing()
	{
		var logger = new RecordingLogger();
		var serializer = Create(JsonMissingMemberHandling.ThrowOnError, logger);
		using var content = new StringContent(FontWithExtraMember);

		var act = () => serializer.FromHttpContentAsync<Font>(content, CancellationToken);

		await act.Should().ThrowAsync<JsonException>();
		logger.Entries.Should().BeEmpty();
	}

	[Fact]
	public async Task ThrowOnError_WithNoMissingMembers_Deserializes()
	{
		var serializer = Create(JsonMissingMemberHandling.ThrowOnError, new RecordingLogger());
		using var content = new StringContent("{\"Id\":3,\"Name\":\"Small\"}");

		var font = await serializer.FromHttpContentAsync<Font>(content, CancellationToken);

		font!.Name.Should().Be("Small");
	}

	[Fact]
	public async Task LogWarningOnErrorAndContinue_LogsAWarningAndReturnsTheObject()
	{
		var logger = new RecordingLogger();
		var actionCalls = 0;
		var serializer = Create(JsonMissingMemberHandling.LogWarningOnErrorAndContinue, logger, action: (_, _, _) => actionCalls++);
		using var content = new StringContent(FontWithExtraMember);

		var font = await serializer.FromHttpContentAsync<Font>(content, CancellationToken);

		font!.Name.Should().Be("Small");
		font.Type.Should().Be(1);
		actionCalls.Should().Be(1);
		logger.MessagesAt(LogLevel.Warning).Should().ContainSingle().Which.Should().Contain("Extra");
	}

	[Fact]
	public async Task LogWarningOnErrorAndContinue_WithNoMissingMembers_LogsNothing()
	{
		var logger = new RecordingLogger();
		var serializer = Create(JsonMissingMemberHandling.LogWarningOnErrorAndContinue, logger);
		using var content = new StringContent("{\"Id\":3}");

		var font = await serializer.FromHttpContentAsync<Font>(content, CancellationToken);

		font!.Id.Should().Be(3);
		logger.Entries.Should().BeEmpty();
	}

	[Fact]
	public async Task UndefinedHandling_IsNotSupported()
	{
		var serializer = Create((JsonMissingMemberHandling)99, new RecordingLogger());
		using var content = new StringContent("{}");

		var act = () => serializer.FromHttpContentAsync<Font>(content, CancellationToken);

		await act.Should().ThrowAsync<NotSupportedException>();
	}

	[Fact]
	public void GetFieldNameForProperty_UsesTheJsonPropertyName()
	{
		var serializer = Create(JsonMissingMemberHandling.Ignore, new RecordingLogger());

		serializer.GetFieldNameForProperty(typeof(Font).GetProperty(nameof(Font.Height))!).Should().Be("High");
	}

	[Fact]
	public async Task ToHttpContent_OmitsNullsAndWritesEnumsAsStrings()
	{
		var serializer = Create(JsonMissingMemberHandling.Ignore, new RecordingLogger());

		using var content = serializer.ToHttpContent(new { Channel = Channel.Lighting, Missing = (string?)null });
		var json = await content.ReadAsStringAsync(CancellationToken);

		json.Should().Be("{\"Channel\":\"Lighting\"}");
	}
}
