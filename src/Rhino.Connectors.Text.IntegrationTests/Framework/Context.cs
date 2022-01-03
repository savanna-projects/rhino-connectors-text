/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System.Collections.Generic;

namespace Rhino.Connectors.Text.IntegrationTests.Framework
{
    /// <summary>
    /// Contract for describing environment information and data.
    /// </summary>
    public class Context
    {
        /// <summary>
        /// Gets or sets the environment context name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the environment systems parameters table. This will usually be
        /// composed of environment parameters and test settings.
        /// </summary>
        public IDictionary<string, object> SystemParams { get; set; }

        /// <summary>
        /// Gets or sets the environment test parameters table. This will usually be
        /// composed of test specific parameters.
        /// </summary>
        public IDictionary<string, object> TestParams { get; set; }
    }
}