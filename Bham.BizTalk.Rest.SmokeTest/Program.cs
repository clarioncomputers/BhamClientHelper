using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

			if (mode == "scenariomissingcert")
			{
				return RunScenarioMissingCert(args);
			}

			if (mode == "scenariotimeoutpublic")
			{
				return RunScenarioTimeoutPublic(args, isXml: false);
			}

			if (mode == "scenariotimeoutpublicxml")
			{
				return RunScenarioTimeoutPublic(args, isXml: true);
			}

			if (mode == "scenariononsuccesshttp")
			{
				return RunScenarioNonSuccessHttp(args, isXml: false);
			}

			if (mode == "scenariononsuccesshttpxml")
			{
				return RunScenarioNonSuccessHttp(args, isXml: true);
			}

			if (mode == "scenariononsuccesshttps")
			{
				return RunScenarioNonSuccessHttps(args, isXml: false);
			}

			if (mode == "scenariononsuccesshttpsxml")
			{
				return RunScenarioNonSuccessHttps(args, isXml: true);
			}

			if (mode == "runfailurescenarios")
			{
				return RunFailureScenarios(args, isXml: false);
			}

			if (mode == "runfailurescenariosxml")
			{
				return RunFailureScenarios(args, isXml: true);
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
		var responseText = ExecutePublicRequest(HttpMethod.Get, url, null, isXml ? "application/xml" : "application/json", timeout);

		Console.WriteLine(isXml ? "PUBLIC GET XML succeeded." : "PUBLIC GET JSON succeeded.");
		Console.WriteLine(responseText);
		return 0;
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
		var responseText = ExecutePublicRequest(new HttpMethod("PATCH"), url, body, mediaType, timeout);

		Console.WriteLine(isXml ? "PUBLIC PATCH XML succeeded." : "PUBLIC PATCH JSON succeeded.");
		Console.WriteLine(responseText);
		return 0;
	}

	private static int RunScenarioMissingCert(string[] args)
	{
		var url = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : "https://localhost/missing-cert";
		var headerName = args.Length > 2 && !string.IsNullOrWhiteSpace(args[2]) ? args[2] : "x-api-key";
		var headerValue = args.Length > 3 && !string.IsNullOrWhiteSpace(args[3]) ? args[3] : "smoke-test";
		var thumbprint = args.Length > 4 && !string.IsNullOrWhiteSpace(args[4]) ? args[4] : "0000000000000000000000000000000000000000";
		var storeLocation = ParseStoreLocation(args, 5, StoreLocation.LocalMachine);
		var storeName = ParseStoreName(args, 6, StoreName.My);
		var timeoutSeconds = ParseInt(args, 7, 30);

		var client = new BizTalkRestClient(
			new BizTalkRestClientSettings
			{
				ApiKeyHeaderName = headerName,
				ApiKeyHeaderValue = headerValue,
				CertThumbprint = thumbprint,
				StoreLocation = storeLocation,
				StoreName = storeName,
				TimeoutSeconds = timeoutSeconds,
				Logger = WriteLog
			});

		try
		{
			client.GetJson(url);
			Console.Error.WriteLine("Missing certificate scenario failed: expected BizTalkRestClientException.");
			return 1;
		}
		catch (BizTalkRestClientException ex)
		{
			if (!(ex.InnerException is InvalidOperationException) || ex.InnerException.Message.IndexOf("Certificate not found", StringComparison.OrdinalIgnoreCase) < 0)
			{
				Console.Error.WriteLine("Missing certificate scenario failed with an unexpected exception shape.");
				Console.Error.WriteLine(ex.ToString());
				return 1;
			}

			Console.WriteLine("MISSING CERTIFICATE scenario passed.");
			Console.WriteLine(ex.InnerException.Message);
			return 0;
		}
	}

	private static int RunScenarioTimeoutPublic(string[] args, bool isXml)
	{
		var delayMilliseconds = ParseInt(args, 1, 5000);
		var timeoutSeconds = ParseInt(args, 2, 1);
		var mediaType = isXml ? "application/xml" : "application/json";

		using (var server = new LocalHttpScenarioServer(200, delayMilliseconds, "{\"status\":\"delayed\"}"))
		{
			try
			{
				ExecutePublicRequest(HttpMethod.Get, server.Url, null, mediaType, timeoutSeconds);
				Console.Error.WriteLine("Timeout scenario failed: expected request timeout.");
				return 1;
			}
			catch (TaskCanceledException ex)
			{
				Console.WriteLine(isXml ? "TIMEOUT XML scenario passed." : "TIMEOUT scenario passed.");
				Console.WriteLine(ex.Message);
				return 0;
			}
		}
	}

	private static int RunScenarioNonSuccessHttp(string[] args, bool isXml)
	{
		var statusCode = ParseInt(args, 1, 503);
		var timeoutSeconds = ParseInt(args, 2, 30);
		var responseBody = args.Length > 3 && !string.IsNullOrWhiteSpace(args[3]) ? args[3] : "{\"error\":\"forced-http-failure\"}";
		var mediaType = isXml ? "application/xml" : "application/json";

		using (var server = new LocalHttpScenarioServer(statusCode, 0, responseBody))
		{
			try
			{
				ExecutePublicRequest(HttpMethod.Get, server.Url, null, mediaType, timeoutSeconds);
				Console.Error.WriteLine("HTTP non-success scenario failed: expected non-success response.");
				return 1;
			}
			catch (HttpRequestException ex)
			{
				if (!ex.Message.StartsWith("Public GET failed:", StringComparison.Ordinal))
				{
					Console.Error.WriteLine("HTTP non-success scenario failed with an unexpected exception shape.");
					Console.Error.WriteLine(ex.ToString());
					return 1;
				}

				Console.WriteLine(isXml ? "HTTP NON-SUCCESS XML scenario passed." : "HTTP NON-SUCCESS scenario passed.");
				Console.WriteLine(ex.Message);
				return 0;
			}
		}
	}

	private static int RunScenarioNonSuccessHttps(string[] args, bool isXml)
	{
		var url = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : "https://httpstat.us/503";
		var timeoutSeconds = ParseInt(args, 2, 30);
		var mediaType = isXml ? "application/xml" : "application/json";

		if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
		{
			Console.Error.WriteLine("HTTPS non-success scenario requires an https:// URL.");
			return 1;
		}

		try
		{
			ExecutePublicRequest(HttpMethod.Get, url, null, mediaType, timeoutSeconds);
			Console.Error.WriteLine("HTTPS non-success scenario failed: expected non-success response.");
			return 1;
		}
		catch (HttpRequestException ex)
		{
			if (!ex.Message.StartsWith("Public GET failed:", StringComparison.Ordinal))
			{
				Console.Error.WriteLine("HTTPS non-success scenario failed before an HTTP response was received.");
				Console.Error.WriteLine(ex.ToString());
				return 1;
			}

			Console.WriteLine(isXml ? "HTTPS NON-SUCCESS XML scenario passed." : "HTTPS NON-SUCCESS scenario passed.");
			Console.WriteLine(ex.Message);
			return 0;
		}
	}

	private static int RunFailureScenarios(string[] args, bool isXml)
	{
		var httpsUrl = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : "https://httpstat.us/503";
		var timeoutMode = isXml ? "scenariotimeoutpublicxml" : "scenariotimeoutpublic";
		var httpMode = isXml ? "scenariononsuccesshttpxml" : "scenariononsuccesshttp";
		var httpsMode = isXml ? "scenariononsuccesshttpsxml" : "scenariononsuccesshttps";
		var timeoutScenario = RunScenarioTimeoutPublic(new[] { timeoutMode, "5000", "1" }, isXml);
		var missingCertScenario = RunScenarioMissingCert(new[] { "scenariomissingcert" });
		var httpScenario = RunScenarioNonSuccessHttp(new[] { httpMode, "503", "30" }, isXml);
		var httpsScenario = RunScenarioNonSuccessHttps(new[] { httpsMode, httpsUrl, "30" }, isXml);

		if (timeoutScenario == 0 && missingCertScenario == 0 && httpScenario == 0 && httpsScenario == 0)
		{
			Console.WriteLine(isXml ? "All XML failure scenarios passed." : "All failure scenarios passed.");
			return 0;
		}

		Console.Error.WriteLine("One or more failure scenarios failed.");
		return 1;
	}

	private static string ExecutePublicRequest(HttpMethod method, string url, string body, string mediaType, int timeoutSeconds)
	{
		using (var client = new HttpClient())
		using (var request = new HttpRequestMessage(method, url))
		{
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
			if (method != HttpMethod.Get)
			{
				request.Content = new StringContent(body ?? string.Empty, Encoding.UTF8, mediaType);
			}

			client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

			using (var response = client.SendAsync(request).GetAwaiter().GetResult())
			{
				var responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (!response.IsSuccessStatusCode)
				{
					throw new HttpRequestException(
						string.Format(
							"Public {0} failed: {1} ({2}). Response: {3}",
							method.Method,
							(int)response.StatusCode,
							response.StatusCode,
							responseText));
				}

				return responseText;
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
			TimeoutSeconds = ParseInt(args, startIndex + 5, 100),
			Logger = WriteLog
		};
	}

	private static void WriteLog(BizTalkRestLogEntry entry)
	{
		if (entry == null)
		{
			return;
		}

		var statusText = entry.StatusCode.HasValue ? " status=" + entry.StatusCode.Value : string.Empty;
		var urlText = string.IsNullOrWhiteSpace(entry.Url) ? string.Empty : " url=" + entry.Url;
		var prefix = string.Format("[{0:O}] [{1}] [{2}]", entry.TimestampUtc, entry.Level, entry.Operation ?? "n/a");
		var line = prefix + statusText + urlText + " " + entry.Message;

		if (entry.Level == BizTalkRestLogLevel.Error)
		{
			Console.Error.WriteLine(line);
			if (entry.Exception != null)
			{
				Console.Error.WriteLine(entry.Exception.ToString());
			}
			return;
		}

		Console.WriteLine(line);
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
		Console.WriteLine("  scenariomissingcert [url] [apiHeaderName] [apiHeaderValue] [thumbprint] [storeLocation] [storeName] [timeoutSeconds]");
		Console.WriteLine("  scenariotimeoutpublic [delayMilliseconds] [timeoutSeconds]");
		Console.WriteLine("  scenariotimeoutpublicxml [delayMilliseconds] [timeoutSeconds]");
		Console.WriteLine("  scenariononsuccesshttp [statusCode] [timeoutSeconds] [responseBody]");
		Console.WriteLine("  scenariononsuccesshttpxml [statusCode] [timeoutSeconds] [responseBody]");
		Console.WriteLine("  scenariononsuccesshttps [httpsUrl] [timeoutSeconds]");
		Console.WriteLine("  scenariononsuccesshttpsxml [httpsUrl] [timeoutSeconds]");
		Console.WriteLine("  runfailurescenarios [httpsNonSuccessUrl]");
		Console.WriteLine("  runfailurescenariosxml [httpsNonSuccessUrl]");
		Console.WriteLine();
		Console.WriteLine("Examples:");
		Console.WriteLine("  getjson https://api.example.com/orders \"customerId=C123&status=Open\" x-api-key abc123 001122... LocalMachine My 100");
		Console.WriteLine("  getxml https://api.example.com/orders - x-api-key abc123 001122... LocalMachine My 100");
		Console.WriteLine("  patchjson https://api.example.com/items/{id} \"{\"\"status\"\":\"\"Done\"\"}\" x-api-key abc123 001122... LocalMachine My 100");
		Console.WriteLine("  patchxml https://api.example.com/items/{id} \"<request><status>Done</status></request>\" x-api-key abc123 001122... LocalMachine My 100");
		Console.WriteLine("  getpublicjson https://jsonplaceholder.typicode.com/posts \"userId=1\" 100");
		Console.WriteLine("  patchpublicjson https://httpbin.org/patch \"{\"\"status\"\":\"\"Done\"\"}\" 100");
		Console.WriteLine("  scenariomissingcert");
		Console.WriteLine("  scenariotimeoutpublic 5000 1");
		Console.WriteLine("  scenariotimeoutpublicxml 5000 1");
		Console.WriteLine("  scenariononsuccesshttp 503 30 \"{\"\"error\"\":\"\"forced-http-failure\"\"}\"");
		Console.WriteLine("  scenariononsuccesshttpxml 503 30 \"<error>forced-http-failure</error>\"");
		Console.WriteLine("  scenariononsuccesshttps https://httpstat.us/503 30");
		Console.WriteLine("  scenariononsuccesshttpsxml https://httpstat.us/503 30");
		Console.WriteLine("  runfailurescenarios https://httpstat.us/503");
		Console.WriteLine("  runfailurescenariosxml https://httpstat.us/503");
	}

	private sealed class LocalHttpScenarioServer : IDisposable
	{
		private readonly HttpListener _listener;
		private readonly Task _requestTask;

		public LocalHttpScenarioServer(int statusCode, int delayMilliseconds, string responseBody)
		{
			if (statusCode <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(statusCode));
			}

			var port = GetAvailablePort();
			Url = string.Format("http://localhost:{0}/", port);
			_listener = new HttpListener();
			_listener.Prefixes.Add(Url);
			_listener.Start();
			_requestTask = Task.Run(() => HandleSingleRequest(statusCode, delayMilliseconds, responseBody));
		}

		public string Url { get; }

		public void Dispose()
		{
			try
			{
				if (_listener.IsListening)
				{
					_listener.Stop();
				}
			}
			catch
			{
			}

			_listener.Close();

			try
			{
				_requestTask.Wait(TimeSpan.FromSeconds(2));
			}
			catch
			{
			}
		}

		private void HandleSingleRequest(int statusCode, int delayMilliseconds, string responseBody)
		{
			try
			{
				var context = _listener.GetContext();
				if (delayMilliseconds > 0)
				{
					Thread.Sleep(delayMilliseconds);
				}

				var bytes = Encoding.UTF8.GetBytes(responseBody ?? string.Empty);
				context.Response.StatusCode = statusCode;
				context.Response.ContentType = "application/json";
				context.Response.ContentLength64 = bytes.Length;
				if (bytes.Length > 0)
				{
					using (var output = context.Response.OutputStream)
					{
						output.Write(bytes, 0, bytes.Length);
					}
				}
				else
				{
					context.Response.Close();
				}
			}
			catch (HttpListenerException)
			{
			}
			catch (ObjectDisposedException)
			{
			}
			catch (IOException)
			{
			}
		}

		private static int GetAvailablePort()
		{
			var listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start();
			try
			{
				return ((IPEndPoint)listener.LocalEndpoint).Port;
			}
			finally
			{
				listener.Stop();
			}
		}
	}
}
