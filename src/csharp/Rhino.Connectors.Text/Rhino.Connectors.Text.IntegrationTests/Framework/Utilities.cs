/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Parser;
using Rhino.Api.Simulator.Framework;

using System;
using System.Reflection;

namespace Rhino.Connectors.Text.IntegrationTests.Framework
{
    /// <summary>
    /// Utilities package for Rhino.Api.UnitTests.
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// Parse Rhino spec into Rhino.Api.Contracts.AutomationProvider.RhinoTestCase.
        /// </summary>
        /// <param name="rhinoSpec">Rhino spec.</param>
        /// <returns>A Rhino.Api.Contracts.AutomationProvider.RhinoTestCase.</returns>
        public static RhinoTestCase Parse(string rhinoSpec)
        {
            // factor text parser
            var textParserType = typeof(RhinoTestCaseFactory).Assembly.GetType("Rhino.Api.Parser.Components.TextParser");
            var textParser = Activator.CreateInstance(textParserType);

            // parse
            return textParserType
                .GetMethod("Parse")
                .Invoke(obj: textParser, new[] { rhinoSpec }) as RhinoTestCase;
        }

        /// <summary>
        /// Gets Rhino Spec stub from Rhino.Api.UnitTests.Framework.
        /// </summary>
        /// <param name="name">Stub name to find by.</param>
        /// <returns>Rhino Spec stub.</returns>
        public static string GetStub(string name)
        {
            return Array
                .Find(typeof(RhinoSpecStub)
                .GetProperties(BindingFlags.Static | BindingFlags.Public), i => i.Name.Equals(name))?
                .GetValue(obj: null)?
                .ToString();
        }
    }
}