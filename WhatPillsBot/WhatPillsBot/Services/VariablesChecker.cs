using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using WhatPillsBot.Model.JSONDeserialization;

namespace WhatPillsBot.Services
{
    public static class VariablesChecker
    {
        private static string SiteVariables { get; set; }

        public static IEnumerable<Shape> Shapes { get; set; }

        public static IEnumerable<Color> Colors { get; set; }

        static VariablesChecker()
        {
            SiteVariables = GetVariablesData();
            Shapes = JsonStringToShapes(SearchShapesJson());
            Colors = JsonStringToColors(SearchColorsJson());
        }

        private static string GetVariablesData()
        {
            const string url = "https://pill-id.webpoisoncontrol.org/js/pill-id.min.js";
            var site = SiteParser.Parser.ParseSiteAsString(url);
            var dataStartIndex = site.IndexOf("return{shapes:") + 14;
            return site.Where((x, i) => i >= dataStartIndex).Aggregate("", (x, y) => x += y);
        }

        private static string SearchShapesJson()
        {
            var colorsIndex = SiteVariables.IndexOf("colors") - 2;
            var shapes = SiteVariables.TakeWhile((x, i) => i <= colorsIndex).Aggregate("", (x, y) => x += y);
            return shapes;
        }

        private static string SearchColorsJson()
        {
            var colorsIndex = SiteVariables.IndexOf("colors") + 7;
            var dataEndIndex = SiteVariables.IndexOf("angular.module") - 5;
            var colors = SiteVariables.Where((x, i) => i >= colorsIndex && i <= dataEndIndex).Aggregate("", (x, y) => x += y);
            return colors;
        }

        public static IEnumerable<Color> JsonStringToColors(string json)
        {
            var colors = JsonConvert.DeserializeObject<IEnumerable<Color>>(json);
            return colors;
        }

        public static IEnumerable<Shape> JsonStringToShapes(string json)
        {
            var shapes = JsonConvert.DeserializeObject<IEnumerable<Shape>>(json);
            return shapes;
        }
    }
}