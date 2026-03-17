using System;
using System.Security.Cryptography.X509Certificates;
using Bham.BizTalk.Rest;

internal static class Program
{
	private static int Main(string[] args)
	{
		if (args == null || args.Length == 0)
		{
			PrintUsage();
			return 1;
		}

		try
		{
			var mode = args[0].Trim().ToLowerInvariant();
			if (mode == "get")
			{
				return RunGet(args);
			}

			if (mode == "patch")
			{
				return RunPatch(args);
			}

			Console.Error.WriteLine("Unknown mode: " + args[0]);
			PrintUsage();
			return 1;
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine("Smoke test failed:");
			Console.Error.WriteLine(ex.ToString());
			return 2;
		}
	}

	private static int RunGet(string[] args)
	{
		// get <url> <apiHeaderName> <apiHeaderValue> <thumbprint> [storeLocation] [storeName] [timeoutSeconds]
		if (args.Length < 5)
		{
			Console.Error.WriteLine("GET requires at least 4 arguments after mode.");
			PrintUsage();
			return 1;
		}

		var url = args[1];
		var apiHeaderName = args[2];
		var apiHeaderValue = args[3];
		var thumbprint = args[4];
		var storeLocation = ParseStoreLocation(args, 5, StoreLocation.LocalMachine);
		var storeName = ParseStoreName(args, 6, StoreName.My);
		var timeout = ParseInt(args, 7, 100);

		var result = PatchClient.GetJsonWithClientCertAndApiKey(
			url,
			apiHeaderName,
			apiHeaderValue,
			thumbprint,
			storeLocation,
			storeName,
			timeout);

		Console.WriteLine("GET succeeded.");
		Console.WriteLine(result);
		return 0;
	}

	private static int RunPatch(string[] args)
	{
		// patch <url> <jsonBody> <apiHeaderName> <apiHeaderValue> <thumbprint> [storeLocation] [storeName] [timeoutSeconds]
		if (args.Length < 6)
		{
			Console.Error.WriteLine("PATCH requires at least 5 arguments after mode.");
			PrintUsage();
			return 1;
		}

		var url = args[1];
		var jsonBody = args[2];
		var apiHeaderName = args[3];
		var apiHeaderValue = args[4];
		var thumbprint = args[5];
		var storeLocation = ParseStoreLocation(args, 6, StoreLocation.LocalMachine);
		var storeName = ParseStoreName(args, 7, StoreName.My);
		var timeout = ParseInt(args, 8, 100);

		var result = PatchClient.PatchJsonWithClientCertAndApiKey(
			url,
			jsonBody,
			apiHeaderName,
			apiHeaderValue,
			thumbprint,
			storeLocation,
			storeName,
			timeout);

		Console.WriteLine("PATCH succeeded.");
		Console.WriteLine(result);
		return 0;
	}

	private static StoreLocation ParseStoreLocation(string[] args, int index, StoreLocation fallback)
	{
		if (args.Length <= index || string.IsNullOrWhiteSpace(args[index]))
		{
			return fallback;
		}

		StoreLocation value;
		return Enum.TryParse(args[index], true, out value) ? value : fallback;
	}

	private static StoreName ParseStoreName(string[] args, int index, StoreName fallback)
	{
		if (args.Length <= index || string.IsNullOrWhiteSpace(args[index]))
		{
			return fallback;
		}

		StoreName value;
		return Enum.TryParse(args[index], true, out value) ? value : fallback;
	}

	private static int ParseInt(string[] args, int index, int fallback)
	{
		if (args.Length <= index || string.IsNullOrWhiteSpace(args[index]))
		{
			return fallback;
		}

		int value;
		return int.TryParse(args[index], out value) ? value : fallback;
	}

	private static void PrintUsage()
	{
		Console.WriteLine("Usage:");
		Console.WriteLine("  get <url> <apiHeaderName> <apiHeaderValue> <thumbprint> [storeLocation] [storeName] [timeoutSeconds]");
		Console.WriteLine("  patch <url> <jsonBody> <apiHeaderName> <apiHeaderValue> <thumbprint> [storeLocation] [storeName] [timeoutSeconds]");
		Console.WriteLine();
		Console.WriteLine("Examples:");
		Console.WriteLine("  get https://api.example.com/items x-api-key abc123 001122... LocalMachine My 100");
		Console.WriteLine("  patch https://api.example.com/items/{id} \"{\"\"status\"\":\"\"Done\"\"}\" x-api-key abc123 001122... LocalMachine My 100");
	}
}
