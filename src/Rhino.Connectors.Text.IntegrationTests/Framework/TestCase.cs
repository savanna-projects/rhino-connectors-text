/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Gravity.Services.DataContracts;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Rhino.Connectors.Text.IntegrationTests.Framework
{
    public abstract class TestCase
    {
        // members: state
        private ConcurrentBag<Context> environments;
        private TestContext testContext;

        #region *** Test: Properties    ***
        /// <summary>
        /// Gets json settings to comply with Rhino API serialization.
        /// </summary>
        public static JsonSerializerSettings JsonSettings
            => Gravity.Extensions.Utilities.GetJsonSettings<CamelCaseNamingStrategy>();

        /// <summary>
        /// Gets the number of attempt for this run.
        /// </summary>
        public int NumberOfAttempts { get; set; } = 1;

        /// <summary>
        /// Gets the delay time in milliseconds between each attempt.
        /// </summary>
        public int AttemptsDelay { get; set; } = 5000;
        #endregion

        #region *** Test: Environments  ***
        public TestCase SetTestContext(TestContext testContext)
        {
            this.testContext = testContext;
            return this;
        }

        /// <summary>
        /// Apply environment(s) to the current Rhino.Connectors.Text.IntegrationTests.Framework.TestCase context.
        /// </summary>
        /// <param name="environments">Applied environment(s).</param>
        /// <returns>Self reference.</returns>
        public TestCase AddEnvironments(params Context[] environments)
        {
            // setup
            this.environments ??= new ConcurrentBag<Context>();

            // append environments
            foreach (var environment in environments)
            {
                DoAddEnvironment(environment);
            }

            // complete pipe
            return this;
        }

        // apply an environment to the current context.
        private void DoAddEnvironment(Context context)
        {
            // setup
            context.SystemParams ??= new Dictionary<string, object>();
            context.TestParams ??= new Dictionary<string, object>();

            // add new environment
            environments.Add(context);
        }
        #endregion

        #region *** Test: Execution     ***
        /// <summary>
        /// Executes a Rhino.Connectors.Text.IntegrationTests.Framework.TestCase against each applied environment.
        /// </summary>
        /// <returns><see cref="true"/> if pass; <see cref="false"/> if not.</returns>
        public bool Execute()
        {
            // no environments conditions
            if (environments == null)
            {
                return ExecuteTestCase(context: default);
            }

            // execute test-case for all environments
            foreach (var environment in PutTestContext())
            {
                var actual = ExecuteTestCase(environment);
                if (actual)
                {
                    return actual;
                }
            }

            // test failed
            return false;
        }

        // put context on environments
        private IEnumerable<Context> PutTestContext()
        {
            // setup
            var onEnvironments = new List<Context>();

            // put
            foreach (var environment in environments)
            {
                var onEnvironment = environment;
                foreach (var key in testContext.Properties.Keys)
                {
                    onEnvironment.SystemParams[$"{key}"] = testContext.Properties[key];
                }
                onEnvironments.Add(onEnvironment);
            }

            // results
            return onEnvironments;
        }

        // executes test-case against a single applied environment
        private bool ExecuteTestCase(Context context)
        {
            // extract number of retires
            _ = int.TryParse($"{testContext.Properties["Integration.NumberOfAttempts"]}", out int attemptsOut);
            NumberOfAttempts = attemptsOut < 1 ? 1 : attemptsOut;

            // execute test-case
            for (int i = 1; i <= NumberOfAttempts; i++)
            {
                var actual = ExecuteIteration(context);
                if (actual)
                {
                    return actual;
                }
            }

            // complete pipeline
            return false;
        }

        // executes test-case iteration against a single applied environment
        private bool ExecuteIteration(Context context)
        {
            try
            {
                // 1. explicit preconditions
                Preconditions(context);

                // 2. execute test case > get actual
                context.TestParams["actual"] = AutomationTest(context);

                // 3. log
                var message = $"TestCase [{GetType().Name}] completed with result [{context.TestParams["actual"]}].";
                testContext.WriteLine(message);
            }
            catch (Exception e) when (e is NotImplementedException || e is AssertInconclusiveException)
            {
                // log
                var p = context.TestParams;
                var message = $"Was unable to conclude results on [{p["driver"]}]";

                // skip test
                Assert.Inconclusive(message);
            }
            catch (Exception e) when (e != null)
            {
                testContext.WriteLine($"Failed to execute [{GetType().Name}] iteration; Reason [{e}]");
                Thread.Sleep(AttemptsDelay);
            }
            finally
            {
                // 4. cleanup
                Cleanup(context);
            }

            // setup conditions
            var isKey = context.TestParams.ContainsKey("actual");

            // test iteration results
            return isKey && (bool)context.TestParams["actual"];
        }

        // executes explicit preconditions
        private void Preconditions(Context context)
        {
            // user
            OnPreconditions(context);
        }

        // executes explicit cleanup
        private void Cleanup(Context context)
        {
            try
            {
                // environment log
                testContext.WriteLine(JsonConvert.SerializeObject(context, Formatting.Indented));

                // user
                OnCleanup(context);
            }
            catch (Exception e) when (e != null)
            {
                testContext.WriteLine($"{e}");
            }
        }

        /// <summary>
        /// Automation test to execute against a single applied Rhino.Connectors.Text.IntegrationTests.Framework.Context.
        /// </summary>
        /// <param name="environment">Applied Rhino.Connectors.Text.IntegrationTests.Framework.Context.</param>
        /// <returns><see cref="true"/> if pass; <see cref="false"/> if not.</returns>
        public abstract bool AutomationTest(Context context);
        #endregion

        #region *** Plugins: Testing    ***
        /// <summary>
        /// Implements preconditions for this Rhino.Connectors.Text.IntegrationTests.Framework.TestCase.
        /// </summary>
        /// <param name="context">Applied environment.</param>
        public virtual void OnPreconditions(Context context)
        {
            // setup
            var onTestsRepository = OnTestsRepository(context);
            var onDriverParameters = OnDriverParameters(context);
            var onConnector = OnConnector(context);
            var onConfigurationContext = OnConfigurationContext(context);
            var onAuthentication = OnAuthentication(context);
            var onEngineConfiguration = OnEngineConfiguration(context);

            // put configuration
            context.TestParams[ContextEntry.Configuration] = new RhinoConfiguration
            {
                Authentication = onAuthentication,
                Context = onConfigurationContext,
                DriverParameters = onDriverParameters.ToList(),
                EngineConfiguration = onEngineConfiguration,
                Name = "Rhino v2 -Integration Tests",
                TestsRepository = onTestsRepository,
                Integration = $"{context.SystemParams["Rhino.Integration"]}",
                ConnectorConfiguration = new RhinoConnectorConfiguration
                {
                    Connector = onConnector
                }
            };
        }

        /// <summary>
        /// Implements cleanup for this Rhino.Connectors.Text.IntegrationTests.Framework.TestCase.
        /// </summary>
        /// <param name="context">Applied environment.</param>
        public virtual void OnCleanup(Context context)
        {
            // Take no action
        }
        #endregion

        #region *** Plugins: Automation ***
        /// <summary>
        /// Gets or sets this request Gravity.Services.DataContracts.Authentication object.
        /// </summary>
        /// <param name="context">Rhino.Connectors.Text.IntegrationTests.Framework.Context to use.</param>
        /// <returns>A new Gravity.Services.DataContracts.Authentication object.</returns>
        public virtual Authentication OnAuthentication(Context context)
        {
            // setup
            var userName = $"{context.SystemParams["Rhino.UserName"]}";
            var password = $"{context.SystemParams["Rhino.Password"]}";

            // results
            return new Authentication
            {
                Password = password,
                Username = userName
            };
        }

        /// <summary>
        /// Gets or sets this request Rhino.Api.Contracts.Configuration.RhinoEngineConfiguration object.
        /// </summary>
        /// <param name="context">Rhino.Connectors.Text.IntegrationTests.Framework.Context to use.</param>
        /// <returns>A new Rhino.Api.Contracts.Configuration.RhinoEngineConfiguration object.</returns>
        public virtual RhinoEngineConfiguration OnEngineConfiguration(Context context)
        {
            // results
            return new RhinoEngineConfiguration
            {
                PageLoadTimeout = 60000,
                ElementSearchingTimeout = 15000
            };
        }

        /// <summary>
        /// Gets or sets Rhino.Api.Contracts.Configuration.DriverParameters object.
        /// </summary>
        /// <param name="context">Rhino.Connectors.Text.IntegrationTests.Framework.Context to use.</param>
        /// <returns>Rhino.Api.Contracts.Configuration.DriverParameters object.</returns>
        public virtual IEnumerable<IDictionary<string, object>> OnDriverParameters(Context context)
        {
            // capabilities
            var capabilities = new Dictionary<string, object>
            {
                ["os"] = "Windows",
                ["os_version"] = "10",
                ["resolution"] = "1920x1080"
            };

            // results
            return new[]
            {
                new Dictionary<string, object>
                {
                    ["driver"] = "ChromeDriver",
                    ["driverBinaries"] = $"{context.SystemParams["Grid.Endpoint.Rhino"]}",
                    ["capabilities"] = capabilities
                }
            };
        }

        /// <summary>
        /// Gets or sets Rhino.Api.Contracts.Configuration.TestsRepository object.
        /// </summary>
        /// <param name="context">Rhino.Connectors.Text.IntegrationTests.Framework.Context to use.</param>
        /// <returns>Rhino.Api.Contracts.Configuration.TestsRepository object.</returns>
        public virtual IEnumerable<string> OnTestsRepository(Context context)
        {
            // setup
            var repositoryBody = $"{context.TestParams["TestsRepository"]}";

            // setup
            return JsonConvert.DeserializeObject<string[]>(repositoryBody, JsonSettings);
        }

        /// <summary>
        /// Gets or sets Rhino.Api.Contracts.Configuration.Context object.
        /// </summary>
        /// <param name="context">Rhino.Connectors.Text.IntegrationTests.Framework.Context to use.</param>
        /// <returns>Rhino.Api.Contracts.Configuration.Context object.</returns>
        public virtual IDictionary<string, object> OnConfigurationContext(Context context)
        {
            // results
            return new Dictionary<string, object>
            {
                ["SkipOnConnect"] = default
            };
        }

        /// <summary>
        /// Gets or sets Rhino.Api.Contracts.Configuration.Connector object.
        /// </summary>
        /// <param name="context">Rhino.Connectors.Text.IntegrationTests.Framework.Context to use.</param>
        /// <returns>Rhino.Api.Contracts.Configuration.Context Connector.</returns>
        public virtual string OnConnector(Context context)
        {
            // results
            return context.TestParams.ContainsKey("Connector")
                ? $"{context.TestParams["Connector"]}"
                : RhinoConnectors.Text;
        }
        #endregion
    }
}