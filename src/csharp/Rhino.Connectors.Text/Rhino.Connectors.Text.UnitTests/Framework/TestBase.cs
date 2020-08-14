/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rhino.Api.Simulator.Framework;

using System.Collections.Generic;
using System.IO;

using Rhino.Api.Contracts.AutomationProvider;
using Gravity.Services.DataContracts;
using System.Linq;

namespace Rhino.Connectors.Text.UnitTests.Framework
{
    [TestClass]
    public abstract class TestBase
    {
        // constants: test case content - for stub "ModelsNoDataSource"
        public const string ExpectedAction1 = "1. go to url {https://gravitymvctestapplication.azurewebsites.net/student}";
        public const string ExpectedAction2 = "2. send keys {@FirstName} into {#SearchString} using {css selector}";
        public const string ExpectedAction3 = "3. click on {#SearchButton} using {css selector}";
        public const string ExpectedAction4 = "4. wait {3000}";
        public const string ExpectedAction5 = "5. register parameter {argument_param} on {#css_id} using {css selector} from {href} filter {regular_expression}";
        public const string ExpectedAction6 = "6. close browser";
        public const string ExpectedResult1 =
            "verify that {url} match {student}\r\n" +
            "verify that {attribute} of {#SearchString} using {css selector} from {value} match {@FirstName}";
        public const string ExpectedResult2 = "verify that {text} of {//TD[contains(@id,'student_last_name_')]} match {@ExpectedLastName}";

        /// <summary>
        /// Gets or sets the <see cref="TestContext"/> object
        /// for Rhino.Connectors.Text.UnitTests.Framework.TestBase
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Gets ChromeDriver driver parameters.
        /// </summary>
        public IDictionary<string, object> ChromeDriver => new Dictionary<string, object>
        {
            ["driver"] = "ChromeDriver",
            ["driverBinaries"] = "http://localhost:4444/wd/hub"
        };

        /// <summary>
        /// Gets GeckoDriver driver parameters.
        /// </summary>
        public IDictionary<string, object> FireFoxDriver => new Dictionary<string, object>
        {
            ["driver"] = "FirefoxDriver",
            ["driverBinaries"] = "http://localhost:4444/wd/hub"
        };

        /// <summary>
        /// Gets a JSON RhinoDataProvider for using with RhinoConfiguration.
        /// </summary>
        public RhinoDataProvider JsonDataProvider => new RhinoDataProvider
        {
            Type = DataSourceType.Json,
            Source = "[" +
            "   {" +
            "       'environment':'test'," +
            "       'searchTextBox':\"//input[@id='SearchString']\"," +
            "       'searchButton':\"//input[@id='SearchString']\"" +
            "   }," +
            "   {" +
            "       'environment':'production'," +
            "       'searchTextBox':\"//input[@id='SearchStringProd']\"," +
            "       'searchButton':\"//input[@id='SearchStringProd']\"" +
            "   }" +
            "]",
            Map = new[]
            {
                new RhinoMapEntry { Test = "DEMO-60727" },
                new RhinoMapEntry { Test = "DEMO-60728" },
                new RhinoMapEntry { Test = "DEMO-60729" }
            }
        };

        /// <summary>
        /// Gets Rhino Test Spec from spec stubs collection.
        /// </summary>
        /// <param name="stubs">Stub name to get Rhino Test Spec by.</param>
        /// <returns>A collection of Rhino Test Spec.</returns>
        public static IEnumerable<string> GetSpecsByStub(params string[] stubs)
        {
            return stubs.Select(Utilities.GetStub);
        }

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