/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rhino.Connectors.Text.IntegrationTests.Framework;
using Rhino.Connectors.Text.IntegrationTests.Cases;

using System.Collections.Generic;

namespace Rhino.Connectors.Text.IntegrationTests.Suites
{
    [TestClass]
    public class ConnectorTests : TestBase
    {
        [DataTestMethod]
        [DataRow("['SpecNoDataSourceIntegration.txt']")]
        public void T0001(string testsRepository)
        {
            // setup            
            var environement = new Context
            {
                TestParams = new Dictionary<string, object>
                {
                    ["TestsRepository"] = testsRepository
                }
            };

            // execute
            var actual = new C0001().AddEnvironments(environement).SetTestContext(TestContext).Execute();

            // assertion
            Assert.IsTrue(actual);
        }
    }
}
