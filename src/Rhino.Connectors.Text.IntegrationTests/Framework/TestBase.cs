/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rhino.Connectors.Text.IntegrationTests.Framework
{
    [TestClass]
    [DeploymentItem("Resources/FullSpecJson.txt")]
    [DeploymentItem("Resources/InputsModel.txt")]
    [DeploymentItem("Resources/ModelsNoDataSource.txt")]
    [DeploymentItem("Resources/SpecNoDataSourceIntegration.txt")]
    [DeploymentItem("Resources/TablesModel.txt")]
    public abstract class TestBase
    {
        /// <summary>
        /// Gets or sets the <see cref="TestContext"/> object
        /// for Rhino.Connectors.Text.UnitTests.Framework.TestBase
        /// </summary>
        public TestContext TestContext { get; set; }
    }
}
