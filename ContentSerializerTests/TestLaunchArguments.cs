using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using LeagueSandbox.ContentSerializer;

namespace LeagueSandbox.ContentSerializerTests
{
    [TestClass]
    public class TestLaunchArguments
    {
        [TestMethod]
        public void TestParse()
        {
            var arguments = new Dictionary<string, string>()
            {
                {"someKey", "someValue"},
                {"secondKey", "secondValue"}
            };

            var formattedArguments = new List<string>();
            foreach(var kvp in arguments)
            {
                formattedArguments.Add(string.Format("--{0}", kvp.Key));
                formattedArguments.Add(kvp.Value);
            }

            var launchArguments = LaunchArguments.Parse(formattedArguments.ToArray());

            Assert.IsFalse(launchArguments.ContainsKey("nonexisting"));

            foreach(var kvp in arguments)
            {
                Assert.IsTrue(launchArguments.ContainsKey(kvp.Key));
                Assert.AreEqual(kvp.Value, launchArguments[kvp.Key]);
            }

            AssertFailParse("Invalid launch argument count", new string[] { "asd" });
            AssertFailParse("Invalid argument format", new string[] { "asd", "asd" });
            AssertFailParse("Invalid argument key length", new string[] { "--", "wef" });
            AssertFailParse("Invalid argument key", new string[] { "---", "wefkwop" });
        }

        private void AssertFailParse(string message, string[] args)
        {
            try
            {
                var launchArguments = LaunchArguments.Parse(args);
            }
            catch (Exception e)
            {
                Assert.AreEqual(message, e.Message);
                return;
            }
            Assert.Fail("No exception was thrown");
        }
    }
}
