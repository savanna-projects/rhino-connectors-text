using System.Text.RegularExpressions;

namespace Rhino.Connectors.Text
{
    internal static partial class FormatRegexes
    {
        [GeneratedRegex(@"\s`$")]
        public static partial Regex Multiline();

        [GeneratedRegex("/^\\d+\\.\\s+/")]
        public static partial Regex ActionNumbers();
    }
}
