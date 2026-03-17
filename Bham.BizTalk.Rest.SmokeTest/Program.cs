using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
			if (mode == "get" || mode == "getjson")
			{
				return RunGet(args, isXml: false);
			}

			if (mode == "getxml")
			{
				return RunGet(args, isXml: true);
			}

			if (mode == "patch" || mode == "patchjson")
			{
				return RunPatch(args, isXml: false);
			}

			if (mode == "patchxml")
			{
				return RunPatch(args, isXml: true);
			}

			if (mode == "getpublic" || mode == "getpublicjson")
			{
				return RunGetPublic(args, isXml: false);
			}

			if (mode == "getpublicxml")
			{
				return RunGetPublic(args, isXml: true);
			}

			if (mode == "patchpublic" || mode == "patchpublicjson")
			{
				return RunPatchPublic(args, isXml: false);
			}

			if (mode == "patchpublicxml")
			{
				return RunPatchPublic(args, isXml: true);
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

	private static int RunGet(string[] args, bool isXml)
	{
		// getjson/getxml <baseUrl> <queryStringOrDash> <apiHeaderName> <apiHeaderValue> <thumbprint> [storeLocation] [storeName] [timeoutSeconds]
		if (args.Length < 6)
		{
			Console.Error.WriteLine("GET requires at least 5 arguments after mode.");
			PrintUsage();
			return 1;
		}

		var baseUrl = args[1];
		var queryParameters = ParseQueryParameters(args[2]);
		var settings = ParseSettings(args, 3);
		var client = new BizTalkRestClient(settings);

		var result = isXml
			? client.GetXml(baseUrl, queryParameters)
			: client.GetJson(baseUrl, queryParameters);

		Console.WriteLine(isXml ? "GET XML succeeded." : "GET JSON succeeded.");
		Console.WriteLine(result);
		return 0;
	}

	private static int RunPatch(string[] args, bool isXml)
	{
		// patchjson/patchxml <url> <body> <apiHeaderName> <apiHeaderValue> <thumbprint> [storeLocation] [storeName] [timeoutSeconds]
		if (args.Length < 6)
		{
			Console.Error.WriteLine("PATCH requires at least 5 arguments after mode.");
			PrintUsage();
			return 1;
		}

		var url = args[1];
		var body = args[2];
		var settings = ParseSettings(args, 3);
		var client = new BizTalkRestClient(settings);

		var result = isXml
			? client.PatchXml(url, body)
			: client.PatchJson(url, body);

		Console.WriteLine(isXml ? "PATCH XML succeeded." : "PATCH JSON succeeded.");
		Console.WriteLine(result);
		return 0;
	}

	private static int RunGetPublic(string[] args, bool isXml)
	{
		// getpublicjson/getpublicxml <baseUrl> <queryStringOrDash> [timeoutSeconds]
		if (args.Length < 3)
		{
			Console.Error.WriteLine("Public GET requires at least 2 arguments after mode.");
			PrintUsage();
			return 1;
		}

		var baseUrl = args[1];
		var queryParameters = ParseQueryParameters(args[2]);
		var timeout = ParseInt(args, 3, 100);
		var url = BizTalkRestClient.BuildUrl(baseUrl, queryParameters);

		using (var client = new HttpClient())
		using (var request = new HttpRequestMessage(HttpMethod.Get, url))
		{
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(isXml ? "application/xml" : "application/json"));
			client.Timeout = TimeSpan.FromSeconds(timeout);

			using (var response = client.SendAsync(request).GetAwaiter().GetResult())
			{
				var responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (!response.IsSuccessStatusCode)
				{
					throw new HttpRequestException(
						string.Format(
							"Public GET failed: {0} ({1}). Response: {2}",
							(int)response.StatusCode,
							response.StatusCode,
							responseText));
				}

				Console.WriteLine(isXml ? "PUBLIC GET XML succeeded." : "PUBLIC GET JSON succeeded.");
				Console.WriteLine(responseText);
				return 0;
			}
		}
	}

	private static int RunPatchPublic(string[] args, bool isXml)
	{
		// patchpublicjson/patchpublicxml <url> <body> [timeoutSeconds]
		if (args.Length < 3)
		{
			Console.Error.WriteLine("Public PATCH requires at least 2 arguments after mode.");
			PrintUsage();
			return 1;
		}

		var url = args[1];
		var body = args[2];
		var timeout = ParseInt(args, 3, 100);
		var mediaType = isXml ? "application/xml" : "application/json";

		using (var client = new HttpClient())
		using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), url))
		{
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
			request.Content = new StringContent(body ?? string.Empty, Encoding.UTF8, mediaType);
			client.Timeout = TimeSpan.FromSeconds(timeout);

			using (var response = client.SendAsync(request).GetAwaiter().GetResult())
			{
				var responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (!response.IsSuccessStatusCode)
				{
					throw new HttpRequestException(
						string.Format(
							"Public PATCH failed: {0} ({1}). Response: {2}",
							(int)response.StatusCode,
							response.StatusCode,
							responseText));
				}

				Console.WriteLine(isXml ? "PUBLIC PATCH XML succeeded." : "PUBLIC PATCH JSON succeeded.");
				Console.WriteLine(responseText);
				return 0;
			}
		}
	}

	private static BizTalkRestClientSettings ParseSettings(string[] args, int startIndex)
	{
		if (args.Length <= startIndex + 2)
		{
			throw new ArgumentException("Missing API header or certificate settings.");
		}

		return new BizTalkRestClientSettings
		{
			ApiKeyHeaderName = args[startIndex],
			ApiKeyHeaderValue = args[startIndex + 1],
			CertThumbprint = args[startIndex + 2],
			StoreLocation = ParseStoreLocation(args, startIndex + 3, StoreLocation.LocalMachine),
			StoreName = ParseStoreName(args, startIndex + 4, StoreName.My),
			TimeoutSeconds = ParseInt(args, startIndex + 5, 100)
		};
	}

	private static IDictionary<string, string> ParseQueryParameters(string queryArg)
	{
		var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (string.IsNullOrWhiteSpace(queryArg) || queryArg == "-")
		{
			return result;
		}

		var pairs = queryArg.Split('&');
		for (var i = 0; i < pairs.Length; i++)
		{
			if (string.IsNullOrWhiteSpace(pairs[i])) continue;

			var separatorIndex = pairs[i].IndexOf('=');
			if (separatorIndex < 0)
			{
				result[pairs[i]] = string.Empty;
				continue;
			}

			var key = pairs[i].Substring(0, separatorIndex);
			var value = pairs[i].Substring(separatorIndex + 1);
			result[key] = value;
		}

		return result;
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
		Console.WriteLine("  getjson <baseUrl> <queryStringOrDash> <apiHeaderName> <apiHeaderValue> <thumbprint> [storeLocation] [storeName] [timeoutSeconds]");
		Console.WriteLine("  getxml <baseUrl> <queryStringOrDash> <apiHeaderName> <apiHeaderValue> <thumbprint> [storeLocation] [storeName] [timeoutSeconds]");
		Console.WriteLine("  patchjson <url> <jsonBody> <apiHeaderName> <apiHeaderValue> <thumbprint> [storeLocation] [storeName] [timeoutSeconds]");
		Console.WriteLine("  patchxml <url> <xmlBody> <apiHeaderName> <apiHeaderValue> <thumbprint> [storeLocation] [storeName] [timeoutSeconds]");
		Console.WriteLine("  getpublicjson <baseUrl> <queryStringOrDash> [timeoutSeconds]");
		Console.WriteLine("  getpublicxml <baseUrl> <queryStringOrDash> [timeoutSeconds]");
		Console.WriteLine("  patchpublicjson <url> <jsonBody> [timeoutSeconds]");
		Console.WriteLine("  patchpublicxml <url> <xmlBody> [timeoutSeconds]");
		Console.WriteLine();
		Console.WriteLine("Examples:");
		Console.WriteLine("  getjson https://api.example.com/orders \"customerId=C123&status=Open\" x-api-key abc123 001122... LocalMachine My 100");
		Console.WriteLine("  getxml https://api.example.com/orders - x-api-key abc123 001122... LocalMachine My 100");
		Console.WriteLine("  patchjson https://api.example.com/items/{id} \"{\"\"status\"\":\"\"Done\"\"}\" x-api-key abc123 001122... LocalMachine My 100");
		Console.WriteLine("  patchxml https://api.example.com/items/{id} \"<request><status>Done</status></request>\" x-api-key abc123 001122... LocalMachine My 100");
		Console.WriteLine("  getpublicjson https://jsonplaceholder.typicode.com/posts \"userId=1\" 100");
		Console.WriteLine("  patchpublicjson https://httpbin.org/patch \"{\"\"status\"\":\"\"Done\"\"}\" 100");
	}
}
