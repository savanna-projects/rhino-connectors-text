/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Abstraction.Logging;

using Rhino.Api;
using Rhino.Api.Contracts.Attributes;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Extensions;
using Rhino.Connectors.Text.Framework;

using System;
using System.Collections.Generic;

namespace Rhino.Connectors.Text
{
    /// <summary>
    /// Text connector for running plain text as Rhino Automation Specs.
    /// </summary>
    [Connector(
        value: Connector.Text,
        Name = "Connector - Plain Text",
        Description = "Allows to run Rhino Specs from a plain text input (files, folders or any other text provider).")]
    public class TextConnector : RhinoConnector
    {
        #region *** Constructors ***
        /// <summary>
        /// Creates a new instance of this Rhino.Api.Components.RhinoConnector.
        /// </summary>
        /// <param name="configuration">Rhino.Api.Contracts.Configuration.RhinoConfiguration to use with this connector.</param>
        public TextConnector(RhinoConfiguration configuration)
            : this(configuration, Utilities.Types)
        { }

        /// <summary>
        /// Creates a new instance of this Rhino.Api.Components.RhinoConnector.
        /// </summary>
        /// <param name="configuration">Rhino.Api.Contracts.Configuration.RhinoConfiguration to use with this connector.</param>
        /// <param name="types">A collection of <see cref="Type"/> to load for this repository.</param>
        public TextConnector(RhinoConfiguration configuration, IEnumerable<Type> types)
            : this(configuration, types, Utilities.CreateDefaultLogger(configuration))
        { }

        /// <summary>
        /// Creates a new instance of this Rhino.Api.Components.RhinoConnector.
        /// </summary>
        /// <param name="configuration">Rhino.Api.Contracts.Configuration.RhinoConfiguration to use with this connector.</param>
        /// <param name="types">A collection of <see cref="Type"/> to load for this repository.</param>
        /// <param name="logger">Gravity.Abstraction.Logging.ILogger implementation for this connector.</param>
        public TextConnector(RhinoConfiguration configuration, IEnumerable<Type> types, ILogger logger)
            : this(configuration, types, logger, connect: true)
        { }

        /// <summary>
        /// Creates a new instance of this Rhino.Api.Components.RhinoConnector.
        /// </summary>
        /// <param name="configuration">Rhino.Api.Contracts.Configuration.RhinoConfiguration to use with this connector.</param>
        /// <param name="types">A collection of <see cref="Type"/> to load for this repository.</param>
        /// <param name="logger">Gravity.Abstraction.Logging.ILogger implementation for this connector.</param>
        /// <param name="connect"><see cref="true"/> for immediately connect after construct <see cref="false"/> skip connection.</param>
        /// <remarks>If you skip connection you must explicitly call Connect method.</remarks>
        public TextConnector(RhinoConfiguration configuration, IEnumerable<Type> types, ILogger logger, bool connect)
            : base(configuration, types, logger)
        {
            // setup provider manager
            ProviderManager = new TextAutomationProvider(configuration, types, logger);

            // connect on constructing
            if (connect)
            {
                Connect();
            }
        }
        #endregion
    }
}