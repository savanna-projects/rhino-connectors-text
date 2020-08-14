/*
 * 0001 - Search Students, Integration Tests
 * 
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
#pragma warning restore
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Extensions;
using Rhino.Connectors.Text.IntegrationTests.Framework;

namespace Rhino.Connectors.Text.IntegrationTests.Cases
{
    public class C0001 : TestCase
    {
        public override bool AutomationTest(Context context)
        {
            // setup
            var configuration = context.TestParams[ContextEntry.Configuration] as RhinoConfiguration;

            // execute
            return configuration.GetConnctor().Execute().Actual;
        }
    }
}