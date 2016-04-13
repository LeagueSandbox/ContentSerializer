using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using LeagueLib.Files;
using System.Diagnostics;

namespace LeagueLibTests.Files
{
    [TestClass]
    public class TestFontConfigFileReader
    {
        [TestMethod]
        public void TestParseLine()
        {
            var successValues = new Dictionary<object[], KeyValuePair<string, string>>
            {
                {
                    new object[] { "tr \"proper example key\" = \"proper example value\"" },
                    new KeyValuePair<string, string>("proper example key", "proper example value")
                },
                {
                    new object[] { "tr\"exampleKeyWithoutSpaces\"=\"exampleValueWithoutSpaces\"" },
                    new KeyValuePair<string, string>("exampleKeyWithoutSpaces", "exampleValueWithoutSpaces")
                },
                {
                    new object[] { "tr   \"rfW#?(¤E=%)%=¤mgepPO8:_*^ÖÅÄ\"    =   \"rfW#?(¤E=%)%=¤mgepPO8:_*^ÖÅÄ\"" },
                    new KeyValuePair<string, string>("rfW#?(¤E=%)%=¤mgepPO8:_*^ÖÅÄ", "rfW#?(¤E=%)%=¤mgepPO8:_*^ÖÅÄ")
                },
                {
                    new object[] { "thStrnggDoesn'tneedformatting\"besidesFor\"¤=%)&(¤=%)(&\"theQuotes\"" },
                    new KeyValuePair<string, string>("besidesFor", "theQuotes")
                }
            };

            var failureValues = new Dictionary<object[], string>
            {
                {
                    new object[] { "some key that has no quotes" },
                    "No such index"
                },
                {
                    new object[] { "some\"key that has only one quote" },
                    "No such index"
                },
                {
                    new object[] { "some\" key that \" has three \" quotes" },
                    "No such index"
                }
            };

            DoSuccessTest(successValues, "ParseLine");
            DoFailureTest(failureValues, "ParseLine");
        }

        [TestMethod]
        public void TestGetQuotedContentAt()
        {
            var successValues = new Dictionary<object[], string>
            {
                {
                    new object[] {"dewhifh\"fjewjofiew\"fewjifowej", 0 },
                    "fjewjofiew"
                },
                {
                    new object[] {"dewhifh\"fjewjofiew\"fe\"wji\"fowej", 0 },
                    "fjewjofiew"
                },
                {
                    new object[] {"dewhifh\"fjewjofiew\"fe\"wji\"fowej", 1 },
                    "wji"
                }
            };

            var failureValues = new Dictionary<object[], string>
            {
                {
                    new object[] {"gjreoi\"grejoi", 0},
                    "No such index"
                },
                {
                    new object[] {"wejio\"gewjrioo\"fgjiow", 1},
                    "No such index"
                }
            };

            DoSuccessTest(successValues, "GetQuotedContentAt");
            DoFailureTest(failureValues, "GetQuotedContentAt");
        }

        [TestMethod]
        public void TestIndexOfAt()
        {
            var successValues = new Dictionary<object[], int>
            {
                {
                    new object[] {"0123.56.8..", '.', 0},
                    4
                },
                {
                    new object[] {"0123.56.8..", '.', 1},
                    7
                },
                {
                    new object[] {"0123.56.8..", '.', 2},
                    9
                },
                {
                    new object[] {"0123.56.8..", '.', 3},
                    10
                },
                {
                    new object[] {"0123.56.8..", '3', 0},
                    3
                },
                {
                    new object[] {"0123\"567", '"', 0},
                    4
                }
            };

            var failureValues = new Dictionary<object[], string>
            {
                {
                    new object[] {"kekekekekeke", 'm', 0 },
                    "No such index"
                },
                {
                    new object[] {"nowForSomethingCompletelyDifferent", 't', 3},
                    "No such index"
                }
            };

            DoSuccessTest(successValues, "IndexOfAt");
            DoFailureTest(failureValues, "IndexOfAt");

        }

        private void DoSuccessTest<T>(Dictionary<object[], T> testData, string method)
        {
            PrivateType fontConfigFile = new PrivateType(typeof(FontConfigFile));
            foreach (var success in testData)
            {
                var result = fontConfigFile.InvokeStatic(method, success.Key);
                Assert.AreEqual(success.Value, result);
            }
        }

        private void DoFailureTest(Dictionary<object[], string> testData, string method)
        {
            PrivateType fontConfigFile = new PrivateType(typeof(FontConfigFile));
            foreach (var failure in testData)
            {
                try
                {
                    fontConfigFile.InvokeStatic(method, failure.Key);
                }
                catch (Exception e)
                {
                    Assert.AreEqual(failure.Value, e.Message);
                }
            }
        }
    }
}
