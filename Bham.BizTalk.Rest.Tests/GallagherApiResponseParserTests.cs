using System;

namespace Bham.BizTalk.Rest.Tests
{
    internal static class GallagherApiResponseParserTests
    {
        public static void GetFirstEntityId_FindsFirstNestedId()
        {
            var responseJson = "{\"results\":[{\"id\":\"629\",\"name\":\"ThirdPartyID\"}]}";

            var result = GallagherApiResponseParser.GetFirstEntityId(responseJson);

            AssertEqual("629", result);
        }

        public static void GetEntityIdByName_FindsMatchingName()
        {
            var responseJson = "{\"results\":[{\"id\":\"662\",\"name\":\"Other\"},{\"id\":\"663\",\"name\":\"6040-CHAMBERLAIN-B-11703\"}]}";

            var result = GallagherApiResponseParser.GetEntityIdByName(responseJson, "6040-CHAMBERLAIN-B-11703");

            AssertEqual("663", result);
        }

        public static void GetAccessGroupMembershipHrefForCardholder_FindsMembershipHref()
        {
            var responseJson = "{\"results\":[{\"href\":\"https://host/api/cardholders/653/access_groups/abc123\",\"cardholder\":{\"id\":\"653\",\"href\":\"https://host/api/cardholders/653\"}}]}";

            var result = GallagherApiResponseParser.GetAccessGroupMembershipHrefForCardholder(responseJson, "653");

            AssertEqual("https://host/api/cardholders/653/access_groups/abc123", result);
        }

        private static void AssertEqual(string expected, string actual)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    string.Format("Assertion failed. Expected: '{0}' Actual: '{1}'", expected, actual));
            }
        }
    }
}