using System;
using System.Linq;
using WhatPillsBot.Services;

namespace WhatPillsBot.Extensions
{
    public static class VariablesSearcher
    {
        public static string SearchColorsIdes(string colorsNames)
        {
            var knownColors = VariablesChecker.Colors;
            string result = null;
            var colors = knownColors.Where(x => colorsNames.IndexOf(x.Name, StringComparison.CurrentCultureIgnoreCase) >= 0).Select(x => x.Value);
            if (colors.Count() > 0)
                result = string.Join(",", colors);
            else
                result = knownColors.Where(x => x.Name.Equals("other")).Select(x => x.Value).FirstOrDefault();
            return result;
        }

        public static string SearchShapeId(string shapeName)
        {
            var knownShapes = VariablesChecker.Shapes;
            string result = null;
            var shapes = knownShapes.Where(x => shapeName.IndexOf(x.Name, StringComparison.CurrentCultureIgnoreCase) >= 0).Select(x => x.Value).FirstOrDefault();
            if (!string.IsNullOrEmpty(shapes))
                result = shapes;
            else
                result = knownShapes.Where(x => x.Name.Equals("other")).Select(x => x.Value).FirstOrDefault();
            return result;
        }
    }
}