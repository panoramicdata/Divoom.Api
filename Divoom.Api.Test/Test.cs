using Microsoft.Extensions.Logging;

namespace Divoom.Api.Test;

// Every test deriving from this class calls the live Divoom cloud API, and needs the device details
// from user secrets, which CI does not have. CI excludes them with --filter "Category!=Integration".
[Trait("Category", "Integration")]
public abstract class Test
{
	protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	protected ILogger Logger { get; }

	protected DivoomClient Client { get; }

	protected Test(ITestOutputHelper testOutputHelper)
	{
		Logger = LoggerFactory.Create(builder => builder
			.AddProvider(new XunitLoggerProvider(testOutputHelper)))
			.CreateLogger<Test>();
		Client = new DivoomClient(TestConfiguration.LoadDivoomClientOptions(), Logger);
	}
}
