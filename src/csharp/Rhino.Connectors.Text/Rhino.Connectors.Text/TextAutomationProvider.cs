/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Extensions;
using Gravity.Services.Comet;

using Newtonsoft.Json;

using Rhino.Api;
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Extensions;
using Rhino.Api.Parser;
using Rhino.Api.Parser.Contracts;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Utilities = Rhino.Api.Extensions.Utilities;

namespace Rhino.Connectors.Text
{
    /// <summary>
    /// Text/Plain connector for using raw text as Rhino Spec.
    /// </summary>
    internal class TextAutomationProvider : ProviderManager
    {
        // state: global parameters
        private readonly ILogger logger;
        private static readonly Orbit client = new Orbit(Utilities.Types);
        private readonly RhinoTestCaseFactory testCaseFactory;

        #region *** Constructors ***
        /// <summary>
        /// Creates a new instance of this Rhino.Api.Simulator.Framework.TextAutomationProvider.
        /// </summary>
        /// <param name="configuration">Rhino.Api.Contracts.Configuration.RhinoConfiguration to use with this provider.</param>
        public TextAutomationProvider(RhinoConfiguration configuration)
            : this(configuration, Utilities.Types)
        { }

        /// <summary>
        /// Creates a new instance of this Rhino.Api.Simulator.Framework.TextAutomationProvider.
        /// </summary>
        /// <param name="configuration">Rhino.Api.Contracts.Configuration.RhinoConfiguration to use with this provider.</param>
        /// <param name="types">A collection of <see cref="Type"/> to load for this repository.</param>
        public TextAutomationProvider(RhinoConfiguration configuration, IEnumerable<Type> types)
            : this(configuration, types, Utilities.CreateDefaultLogger(configuration))
        { }

        /// <summary>
        /// Creates a new instance of this Rhino.Api.Simulator.Framework.TextAutomationProvider.
        /// </summary>
        /// <param name="configuration">Rhino.Api.Contracts.Configuration.RhinoConfiguration to use with this provider.</param>
        /// <param name="types">A collection of <see cref="Type"/> to load for this repository.</param>
        /// <param name="logger">Gravity.Abstraction.Logging.ILogger implementation for this provider.</param>
        public TextAutomationProvider(RhinoConfiguration configuration, IEnumerable<Type> types, ILogger logger)
            : base(configuration, types, logger)
        {
            this.logger = logger?.Setup(loggerName: nameof(TextAutomationProvider));
            testCaseFactory = new RhinoTestCaseFactory(client, logger);
        }
        #endregion

        #region *** Test Cases   ***
        /// <summary>
        /// Returns a list of test cases for a project.
        /// </summary>
        /// <param name="ids">A list of test ids to get test cases by.</param>
        /// <returns>A collection of Rhino.Api.Contracts.AutomationProvider.RhinoTestCase</returns>
        public override IEnumerable<RhinoTestCase> OnGetTestCases(params string[] ids)
        {
            return ids.SelectMany(LoadRepository);
        }

        // load all test-cases from the given repository
        private IEnumerable<RhinoTestCase> LoadRepository(string repository)
        {
            // setup conditions
            var isDirectory = Directory.Exists(repository);
            var isFile = !isDirectory && File.Exists(repository);

            // exit conditions
            if (!isDirectory && !isFile)
            {
                return LoadFromPlainText(repository);
            }

            // load test-cases
            return isFile ? LoadFromFile(repository) : LoadFromFolder(repository);
        }

        // loads all test-cases from a single folder
        private IEnumerable<RhinoTestCase> LoadFromFolder(string folder)
        {
            // load test cases
            var files = Directory.GetFiles(folder);
            var testCases = files.SelectMany(LoadFromFile);

            // results
            logger?.DebugFormat("Total of [{0}] files loaded from [{1}].", files?.Count(), folder);
            return testCases;
        }

        // loads all test-cases from a single file
        private IEnumerable<RhinoTestCase> LoadFromFile(string file)
        {
            // constants
            var separator = new string[] { SpecSection.Separator };
            const StringSplitOptions SplitOptions = StringSplitOptions.RemoveEmptyEntries;

            // read file
            var raw = File.ReadAllText(file).Trim();
            var loadedTestCase = raw.Split(separator, SplitOptions);

            // parse test-cases
            var cases = testCaseFactory.GetTestCases(loadedTestCase);
            logger?.DebugFormat("Total of [{0}] test-cases loaded from \n{1}\n", cases.Count(), file);

            // results
            return cases;
        }

        // loads a row test case from this repository
        private IEnumerable<RhinoTestCase> LoadFromPlainText(string testCase)
        {
            // parse test-cases
            var cases = testCaseFactory.GetTestCases(testCase);
            logger?.DebugFormat("Total of [{0}] test-cases loaded from \n{1}\n", cases.Count(), testCase);

            // results
            return cases.Where(i => i.Steps.Any());
        }
        #endregion

        #region *** Page Models  ***
        /// <summary>
        /// Returns a collection of <see cref="IDictionary{TKey, TValue}"/> which represents pages models.
        /// </summary>
        /// <param name="sources">Sources from or by which to load page models.</param>
        /// <returns>A collection of Rhino.Api.Contracts.AutomationProvider.RhinoPageModel.</returns>
        public override IEnumerable<RhinoPageModel> GetPageModels(IEnumerable<string> sources)
        {
            // setup: entries
            var modelsBody = sources.SelectMany(Get);

            // build
            var models = new List<RhinoPageModel>();
            foreach (var model in modelsBody)
            {
                var isJsonArray = model.IsJson() && model.StartsWith("[") && model.EndsWith("]");
                var onModels = isJsonArray
                    ? JsonConvert.DeserializeObject<RhinoPageModel[]>(model)
                    : new[] { JsonConvert.DeserializeObject<RhinoPageModel>(model) };
                models.AddRange(onModels);
            }

            // results
            return models;
        }
        #endregion

        // UTILITIES
        // gets all content under the provided source
        private IEnumerable<string> Get(string source)
        {
            // setup conditions
            var isDirectory = Directory.Exists(source);
            var isFile = !isDirectory && File.Exists(source);

            // exit conditions
            if (!isDirectory && !isFile)
            {
                return new[] { source };
            }

            // load test-cases
            return isFile
                ? new[] { File.ReadAllText(source).Trim() }
                : GetByFolder(source);
        }

        // gets all content under the provided folder
        private IEnumerable<string> GetByFolder(string source)
        {
            // load test cases
            var files = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);
            var data = files.Select(i => File.ReadAllText(i).Trim());

            // results
            logger?.DebugFormat("Total of [{0}] files loaded from [{1}].", files?.Count(), source);
            return data;
        }
    }
}