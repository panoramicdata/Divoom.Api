using Divergic.Logging.Xunit;
using Newtonsoft.Json;
using System.Configuration;
using Xunit.Abstractions;

namespace Divoom.Api.Test;

public abstract class Test
{
	private DivoomClientOptions? _divoomClientOptions;

	protected ICacheLogger Logger { get; }

	protected DivoomClient Client { get; }

	public Test(ITestOutputHelper testOutputHelper)
	{
		Logger = testOutputHelper.BuildLogger();
		Client = new DivoomClient(GetDivoomClientOptions(), Logger);
	}

	private DivoomClientOptions GetDivoomClientOptions()
	{
		// Have we already created this?
		if (_divoomClientOptions is not null)
		{
			// Yes - return that one
			return _divoomClientOptions;
		}
		// No - we need to create one

		// Load from file
		var fileInfo = new FileInfo("../../../appsettings.json");

		// Does the file exist?
		if (!fileInfo.Exists)
		{
			// No - hint to the user what to do
			throw new ConfigurationErrorsException("Missing appsettings.json.  Please copy the appsettings.example.json in the project root folder and set the various values appropriately.");
		}
		// Yes

		// Load in the config
		_divoomClientOptions = JsonConvert.DeserializeObject<DivoomClientOptions>(File.ReadAllText(fileInfo.FullName));
		if (_divoomClientOptions is null)
		{
			throw new ConfigurationErrorsException("Configuration did not deserialize");
		}

		return _divoomClientOptions;
	}
}
