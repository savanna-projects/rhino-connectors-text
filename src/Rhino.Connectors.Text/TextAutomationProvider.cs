/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using Gravity.Abstraction.Logging;
using Gravity.Extensions;

using Rhino.Api;
using Rhino.Api.Contracts;
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Api.Converters;
using Rhino.Api.Extensions;
using Rhino.Api.Parser;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

using static System.Runtime.InteropServices.JavaScript.JSType;

using Utilities = Rhino.Api.Extensions.Utilities;

namespace Rhino.Connectors.Text
{
    /// <summary>
    /// Text/Plain connector for using raw text as Rhino Spec.
    /// </summary>
    public partial class TextAutomationProvider : ProviderManager
    {
        // state: global parameters
        private readonly ILogger logger;
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
            testCaseFactory = new RhinoTestCaseFactory(logger);
        }
        #endregion

        #region *** Test Cases   ***
        /// <summary>
        /// Returns a list of test cases for a project.
        /// </summary>
        /// <param name="ids">A list of test ids to get test cases by.</param>
        /// <returns>A collection of Rhino.Api.Contracts.AutomationProvider.RhinoTestCase</returns>
        protected override IEnumerable<RhinoTestCase> OnGetTestCases(params string[] ids)
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
            logger?.DebugFormat("Total of [{0}] files loaded from [{1}].", files?.Length, folder);
            return testCases;
        }

        // loads all test-cases from a single file
        private IEnumerable<RhinoTestCase> LoadFromFile(string file)
        {
            // constants
            var separator = new string[] { RhinoSpecification.Separator };
            const StringSplitOptions SplitOptions = StringSplitOptions.RemoveEmptyEntries;

            // read file
            var raw = File.ReadAllText(file).Trim();
            var loadedTestCases = raw.Split(separator, SplitOptions);

            // normalize
            for(int i = 0; i < loadedTestCases.Length; i++)
            {
                loadedTestCases[i] = FormatRhinoSpec(loadedTestCases[i]);
            }

            // parse test-cases
            var cases = testCaseFactory.GetTestCases(loadedTestCases);
            logger?.DebugFormat("Total of [{0}] test-cases loaded from \n{1}\n", cases.Count(), file);

            // results
            return cases;
        }

        // loads a row test case from this repository
        private IEnumerable<RhinoTestCase> LoadFromPlainText(string testCase)
        {
            // normalize
            testCase = FormatRhinoSpec(testCase);

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
        protected override IEnumerable<RhinoPageModel> OnGetPageModels(IEnumerable<string> sources)
        {
            // setup: entries
            var modelsBody = sources.SelectMany(Get);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            // build
            var models = new List<RhinoPageModel>();
            foreach (var model in modelsBody)
            {
                if (!model.IsJson())
                {
                    continue;
                }
                var isJsonArray = model.StartsWith("[") && model.EndsWith("]");
                var onModels = isJsonArray
                    ? JsonSerializer.Deserialize<RhinoPageModel[]>(model, options)
                    : new[] { JsonSerializer.Deserialize<RhinoPageModel>(model, options) };
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
            logger?.DebugFormat("Total of [{0}] files loaded from [{1}].", files?.Length, source);
            return data;
        }

        private static string FormatRhinoSpec(string testCase)
        {
            List<string> lines = ReplaceRhinoNewLines(testCase);
            var formattedLines = RemoveActionNumbers(lines);
            return string.Join(Environment.NewLine, formattedLines);

            static List<string> ReplaceRhinoNewLines(string testCase)
            {
                // constants
                var multilineRegex = FormatRegexes.Multiline();

                // setup
                var rawLines = testCase.Split(Environment.NewLine);
                var lines = new List<string>();

                // normalize
                for (int i = 0; i < rawLines.Length; i++)
                {
                    var line = rawLines[i];
                    var previousLine = rawLines[i - 1 < 0 ? 0 : i - 1];
                    var isMatch = multilineRegex.IsMatch(line.Trim());
                    var isPreviousMatch = multilineRegex.IsMatch(previousLine.Trim());

                    if (!isMatch && !isPreviousMatch || isMatch && !isPreviousMatch)
                    {
                        lines.Add(line);
                        continue;
                    }

                    var index = lines.Count - 1;
                    var multiline = lines[index];

                    line = ' ' + multilineRegex.Replace(line.Trim(), string.Empty);
                    multiline = multilineRegex.Replace(multiline.Trim(), string.Empty) + line;

                    lines[index] = multiline;
                }

                return lines;
            }

            static IEnumerable<string> RemoveActionNumbers(List<string> lines)
            {
                return lines.Select(line => FormatRegexes.ActionNumbers().Replace(line.Trim(), string.Empty));
            }
        }


    }
}
