/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Simulator.Framework;
using Rhino.Connectors.Text.UnitTests.Framework;

using System;
using System.Linq;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using Utilities = Rhino.Api.Extensions.Utilities;

namespace Rhino.Connectors.Text.UnitTests.ConnectorApi
{
    [TestClass]
    public class ConnectorTests : TestBase
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ConnectNoDriverParams()
        {
            // setup
            var configuration = new RhinoConfiguration
            {
                TestsRepository = GetSpecsByStub("FullSpecJson", "FullSpecMarkdownV1", "FullSpecMarkdownV2")
            };

            // execute
            var testCases = new TextConnector(configuration).ProviderManager.TestRun.TestCases;

            // assert
            Assert.IsFalse(testCases.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ConnectNoTestCases()
        {
            // execute
            var testCases = new TextConnector(new RhinoConfiguration(), Utilities.Types).ProviderManager.TestRun.TestCases;

            // assert
            Assert.IsFalse(testCases.Any());
        }

        [DataTestMethod]
        [DataRow("FullSpecNoDataSource", 1)]
        [DataRow("FullSpecJson", 3)]
        public void ConnectTestsCount(string rhinoSpec, int expected)
        {
            // setup
            var configuration = new RhinoConfiguration
            {
                TestsRepository = GetSpecsByStub(rhinoSpec),
                DriverParameters = new[]
                {
                    ChromeDriver
                }
            };

            // execute
            var testCases = new TextConnector(configuration, Utilities.Types).ProviderManager.TestRun.TestCases;

            // assert
            Assert.AreEqual(expected, actual: testCases.Count());
        }

        [TestMethod]
        public void ConnectTestsCountDriverParams()
        {
            // setup
            var configuration = new RhinoConfiguration
            {
                TestsRepository = GetSpecsByStub("FullSpecJson", "FullSpecMarkdownV1", "FullSpecMarkdownV2"),
                DriverParameters = new[]
                {
                    ChromeDriver,
                    FireFoxDriver
                }
            };

            // execute
            var testCases = new TextConnector(configuration, Utilities.Types).ProviderManager.TestRun.TestCases;

            // assert
            Assert.AreEqual(expected: 18, actual: testCases.Count());
        }

        [TestMethod]
        public void ConnectTestsCountLocalDataSource()
        {
            // setup
            var configuration = new RhinoConfiguration
            {
                TestsRepository = GetSpecsByStub("FullSpecJson", "FullSpecMarkdownV1", "FullSpecMarkdownV2"),
                DriverParameters = new[]
                {
                    ChromeDriver
                }
            };

            // execute
            var testCases = new TextConnector(configuration, Utilities.Types).Connect().ProviderManager.TestRun.TestCases;

            // assert
            Assert.AreEqual(expected: 9, actual: testCases.Count());
        }

        [TestMethod]
        public void ConnectTestsCountGlobalDataSource()
        {
            // setup
            var configuration = new RhinoConfiguration
            {
                TestsRepository = GetSpecsByStub("FullSpecJson", "FullSpecMarkdownV1", "FullSpecMarkdownV2"),
                DriverParameters = new[]
                {
                    ChromeDriver
                },
                DataSource = new[]
                {
                    JsonDataProvider
                }
            };

            // execute
            var testCases = new TextConnector(configuration, Utilities.Types).ProviderManager.TestRun.TestCases;

            // assert
            Assert.AreEqual(expected: 18, actual: testCases.Count());
        }

        [DataTestMethod]
        [DataRow(0, ExpectedAction1, "")]
        [DataRow(1, ExpectedAction2, ExpectedResult1)]
        [DataRow(2, ExpectedAction3, "")]
        [DataRow(3, ExpectedAction4, ExpectedResult2)]
        [DataRow(4, ExpectedAction5, "")]
        [DataRow(5, ExpectedAction6, "")]
        public void ConnectPageModelsContent(int step, string action, string expected)
        {
            // constants
            const string TestKey = "MODELS-60727";

            // setup
            var configuration = new RhinoConfiguration
            {
                TestsRepository = GetSpecsByStub("ModelsNoDataSource"),
                Models = new[] { RhinoModelStub.InputsModel, RhinoModelStub.TablesModel },
                DriverParameters = new[]
                {
                    ChromeDriver
                }
            };

            // execute
            var actualTestStep = new TextConnector(configuration, Utilities.Types)
                .ProviderManager
                .TestRun
                .TestCases
                .First(i => i.Key == TestKey)
                .Steps
                .ElementAt(step);

            // get actuals
            var actualAction = actualTestStep.Action;
            var actualExpected = actualTestStep.Expected;

            // assert
            Assert.AreEqual(expected: action, actual: actualAction);
            Assert.AreEqual(expected: expected, actual: actualExpected);
        }

        [DataTestMethod]
        [DataRow(0, ExpectedAction1, "")]
        [DataRow(1, ExpectedAction2, ExpectedResult1)]
        [DataRow(2, ExpectedAction3, "")]
        [DataRow(3, ExpectedAction4, ExpectedResult2)]
        [DataRow(4, ExpectedAction5, "")]
        [DataRow(5, ExpectedAction6, "")]
        public void ConnectPageModelsContentFromFiles(int step, string action, string expected)
        {
            // constants
            const string TestKey = "MODELS-60727";

            // setup
            var configuration = new RhinoConfiguration
            {
                TestsRepository = new[] { "ModelsNoDataSource.txt" },
                Models = new[] { "InputsModel.txt", "TablesModel.txt" },
                DriverParameters = new[]
                {
                    ChromeDriver
                }
            };

            // execute
            var actualTestStep = new TextConnector(configuration, Utilities.Types)
                .ProviderManager
                .TestRun
                .TestCases
                .First(i => i.Key == TestKey)
                .Steps
                .ElementAt(step);

            // get actuals
            var actualAction = actualTestStep.Action;
            var actualExpected = actualTestStep.Expected;

            // assert
            Assert.AreEqual(expected: action, actual: actualAction);
            Assert.AreEqual(expected: expected, actual: actualExpected);
        }

        [TestMethod]
        public void ConnectPageModelsCount()
        {
            // constants
            const string TestKey = "MODELS-60727";

            // setup
            var configuration = new RhinoConfiguration
            {
                TestsRepository = GetSpecsByStub("ModelsNoDataSource"),
                Models = new[] { RhinoModelStub.InputsModel, RhinoModelStub.TablesModel },
                DriverParameters = new[]
                {
                    ChromeDriver
                }
            };

            // execute
            var actual = new TextConnector(configuration, Utilities.Types)
                .ProviderManager
                .TestRun
                .TestCases
                .First(i => i.Key == TestKey)
                .ModelEntries
                .Count();

            // assert
            Assert.AreEqual(expected: 3, actual);
        }

        [TestMethod]
        public void ConnectDataSourceCount()
        {
            // constants
            const string TestKey = "DEMO-60727";

            // setup
            var configuration = new RhinoConfiguration
            {
                TestsRepository = GetSpecsByStub("FullSpecJson"),
                Models = new[] { RhinoModelStub.InputsModel, RhinoModelStub.TablesModel },
                DriverParameters = new[]
                {
                    ChromeDriver
                }
            };

            // execute
            var actual = new TextConnector(configuration, Utilities.Types)
                .ProviderManager
                .TestRun
                .TestCases
                .Count(i => i.Key == TestKey);

            // assert
            Assert.AreEqual(expected: 3, actual);
        }

        [TestMethod]
        public void ConnectDataSourceCountFromFile()
        {
            // constants
            const string TestKey = "DEMO-60727";

            // setup
            var configuration = new RhinoConfiguration
            {
                TestsRepository = new[] { "FullSpecJson.txt" },
                Models = new[] { RhinoModelStub.InputsModel, RhinoModelStub.TablesModel },
                DriverParameters = new[]
                {
                    ChromeDriver
                }
            };

            // execute
            var actual = new TextConnector(configuration, Utilities.Types)
                .ProviderManager
                .TestRun
                .TestCases
                .Count(i => i.Key == TestKey);

            // assert
            Assert.AreEqual(expected: 3, actual);
        }

        [DataTestMethod]
        [DataRow(0, "Carson", "Alexander")]
        [DataRow(1, "Meredith", "Alonso")]
        [DataRow(2, "Arturo", "Anand")]
        public void ConnectDataSourceContent(int iteration, string firstName, string lastName)
        {
            // constants
            const string TestKey = "DEMO-60727";

            // setup
            var configuration = new RhinoConfiguration
            {
                TestsRepository = GetSpecsByStub("FullSpecJson"),
                Models = new[] { RhinoModelStub.InputsModel, RhinoModelStub.TablesModel },
                DriverParameters = new[]
                {
                    ChromeDriver
                }
            };

            // execute
            var actual = new TextConnector(configuration, Utilities.Types)
                .ProviderManager
                .TestRun
                .TestCases
                .Where(i => i.Key == TestKey)
                .ElementAt(iteration);
            var asString = JsonConvert.SerializeObject(actual);

            // assert
            Assert.IsTrue(asString.Contains(firstName));
            Assert.IsTrue(asString.Contains(lastName));
        }

        [DataTestMethod]
        [DataRow(0, "Carson", "Alexander")]
        [DataRow(1, "Meredith", "Alonso")]
        [DataRow(2, "Arturo", "Anand")]
        public void ConnectDataSourceContentFromFiles(int iteration, string firstName, string lastName)
        {
            // constants
            const string TestKey = "DEMO-60727";

            // setup
            var configuration = new RhinoConfiguration
            {
                TestsRepository = new[] { "FullSpecJson.txt" },
                Models = new[] { "InputsModel.txt", "TablesModel.txt" },
                DriverParameters = new[]
                {
                    ChromeDriver
                }
            };

            // execute
            var actual = new TextConnector(configuration, Utilities.Types)
                .ProviderManager
                .TestRun
                .TestCases
                .Where(i => i.Key == TestKey)
                .ElementAt(iteration);
            var asString = JsonConvert.SerializeObject(actual);

            // assert
            Assert.IsTrue(asString.Contains(firstName));
            Assert.IsTrue(asString.Contains(lastName));
        }
    }
}