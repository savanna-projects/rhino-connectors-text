/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rhino.Api.Simulator.Framework;

using System.IO;

namespace Rhino.Connectors.Text.IntegrationTests.Framework
{
    [TestClass]
    public abstract class TestBase
    {
        /// <summary>
        /// Gets or sets the <see cref="TestContext"/> object
        /// for Rhino.Connectors.Text.UnitTests.Framework.TestBase
        /// </summary>
        public TestContext TestContext { get; set; }

        [AssemblyInitialize]
        public static void Setup(TestContext context)
        {
            // write files
            File.WriteAllText("ModelsNoDataSource.txt", Utilities.GetStub(name: "ModelsNoDataSource"));
            File.WriteAllText("FullSpecJson.txt", Utilities.GetStub(name: "FullSpecJson"));
            File.WriteAllText("InputsModel.txt", RhinoModelStub.InputsModel);
            File.WriteAllText("TablesModel.txt", RhinoModelStub.TablesModel);

            // log
            context.WriteLine("Class setup completed");
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            // clean files
            File.Delete("ModelsNoDataSource.txt");
            File.Delete("FullSpecJson.txt");
            File.Delete("InputsModel.txt");
            File.Delete("TablesModel.txt");
        }
    }
}
