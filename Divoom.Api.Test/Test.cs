using Microsoft.Extensions.Logging;

namespace Divoom.Api.Test;

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
