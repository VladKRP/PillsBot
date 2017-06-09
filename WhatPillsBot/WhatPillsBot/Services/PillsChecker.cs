using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using WhatPillsBot.Model;

namespace WhatPillsBot.Services
{
    public static class PillsChecker
    {
        public static IEnumerable<Pill> GetPills()
        {
            string siteUrl = "https://pill-id.webpoisoncontrol.org/#/tab-name?q=aba";
            var sitePill = SiteParser.Parser.ParseSiteAsString(siteUrl);
            string url = "https://pill-id.webpoisoncontrol.org/js/pill-id.min.js";   
            var site = SiteParser.Parser.ParseSiteAsString(url);
            var dataStartIndex = site.IndexOf("return{shapes:");
            var data = site.Where((x,i) => i >= dataStartIndex).Aggregate("",(x,y) => x+=y);
            var colorsIndex = data.IndexOf("colors");
            var dataEndIndex = data.IndexOf("angular.module");
            var shape = data.TakeWhile((x, i) => i <=  colorsIndex).Aggregate("", (x, y) => x += y); ;     
            var colors = data.Where((x, i) => i >= colorsIndex && i <= dataEndIndex).Aggregate("", (x, y) => x += y);
            var cl = GetExistingPillColors(colors);
            return null;
        }

        public static IEnumerable<string> GetExistingPillColors(string colors)
        {
            var bracket = "{";
            var bracketIndex = Regex.Matches(colors, bracket).Cast<Match>().Select(m => m.Index + 1);
            return null;
        }

        public static IEnumerable<string> GetExistingPillShapes(string shapes)
        {
            return null;
        }


    }
}