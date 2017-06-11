using Newtonsoft.Json;
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
        
        public static Pill GetPill(string id)
        {
            //string pillUrl = $"https://api.webpoisoncontrol.org/api/pill/{id}?caseId=68920";
            string pillUrl = "https://api.webpoisoncontrol.org/api/pill/36961?caseId=68920";
            const string referer = "https://pill-id.webpoisoncontrol.org/";
            var sitePill = SiteParser.Parser.SendRequest(pillUrl, "GET", referer);
            var pill = JsonConvert.DeserializeObject<Pill>(sitePill);
            return pill;
        }

        public static IEnumerable<Pill> GetPills(UserRequest request)
        {
            IEnumerable<Pill> pills = Enumerable.Empty<Pill>();
            if(request != null)
            {
                string pillsUrl = $"https://api.webpoisoncontrol.org/api/pillid/?a={request.FrontSideId}&b={request.BackSideId}&colors={request.Colors}&shapes={request.Shape}&q={request.Name}";
                //string pillsUrl = $"https://api.webpoisoncontrol.org/api/pillid/?a=10&b=&colors=&shapes=&q=";
                const string referer = "https://pill-id.webpoisoncontrol.org/";
                var sitePills = SiteParser.Parser.SendRequest(pillsUrl, "GET", referer);
                pills = JsonConvert.DeserializeObject<IEnumerable<Pill>>(sitePills);
            }
            return pills;
        }

    }
}