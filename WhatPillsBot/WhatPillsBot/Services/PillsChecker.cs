using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WhatPillsBot.Model;

namespace WhatPillsBot.Services
{
    public static class PillsChecker
    {
        public static IEnumerable<Pill> GetPills()
        {
            string url = "https://pill-id.webpoisoncontrol.org/#/tab-imprint";
            var site = SiteParser.Parser.ParseSiteAsString(url);
            return null;
        }
    }
}