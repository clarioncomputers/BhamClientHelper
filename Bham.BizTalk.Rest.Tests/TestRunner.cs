using System;

namespace Bham.BizTalk.Rest.Tests
{
    public static class TestRunner
    {
        public static void RunAll()
        {
            Run(nameof(BizTalkRestClientTests.BuildUrl_AppendsEncodedQueryParameters), BizTalkRestClientTests.BuildUrl_AppendsEncodedQueryParameters);
            Run(nameof(BizTalkRestClientTests.BuildUrl_AppendsWithAmpersand_WhenBaseAlreadyHasQuery), BizTalkRestClientTests.BuildUrl_AppendsWithAmpersand_WhenBaseAlreadyHasQuery);
            Run(nameof(BizTalkRestClientTests.GetJson_ThrowsArgumentException_WhenUrlIsNotHttpOrHttps), BizTalkRestClientTests.GetJson_ThrowsArgumentException_WhenUrlIsNotHttpOrHttps);
        }

        private static void Run(string name, Action test)
        {
            try
            {
                test();
                Console.WriteLine("PASS: " + name);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("FAIL: " + name, ex);
            }
        }
    }
}
