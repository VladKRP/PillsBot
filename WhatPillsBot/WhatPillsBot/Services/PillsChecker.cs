using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using WhatPillsBot.Model.JSONDeserialization;
using WhatPillsBot.Model;

namespace WhatPillsBot.Services
{
    public static class PillsChecker
    {
        const string referer = "https://pill-id.webpoisoncontrol.org/";

        public static Pill GetPill(string id)
        {
            string pillUrl = $"https://api.webpoisoncontrol.org/api/pill/{id}?caseId=1";
            var sitePill = SiteParser.Parser.SendRequest(pillUrl, "GET", referer);
            var pill = JsonConvert.DeserializeObject<Pill>(sitePill);
            return pill;
        }

        public static string GetPillUsage(string id)
        {
            string pillUrl = $"https://api.webpoisoncontrol.org/api/pill/{id}?caseId=1";
            var sitePill = SiteParser.Parser.SendRequest(pillUrl, "GET", referer);
            var pill = JsonConvert.DeserializeObject<Pill>(sitePill);
            return pill.Usage.Replace("<p>","").Replace("</p>","");
        }

        public static IEnumerable<PillGroup> GetPillGroups(string name)
        {
            IEnumerable<PillGroup> groups = Enumerable.Empty<PillGroup>();
            var pillSearch = GetPillSearchResultByName(name);
            if (isAnyGroupsReturned(pillSearch))
                groups = pillSearch.Groups;
            return groups;
        }

        public static IEnumerable<Pill> GetPillProducts(string name)
        {
            IEnumerable<Pill> pills = Enumerable.Empty<Pill>();
            var pillSearch = GetPillSearchResultByName(name);
            if (isAnyProductsReturned(pillSearch))
               pills = pillSearch.Products;
            return pills;
        }

        public static IEnumerable<Pill> GetPillByGroupIdAndName(string id, string name)
        {
            IEnumerable<Pill> pills = Enumerable.Empty<Pill>();
            if(!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name))
            {
                string pillsUrl = $"https://api.webpoisoncontrol.org/api/pillgroups?id={id}&search={name}";
                var sitePills = SiteParser.Parser.SendRequest(pillsUrl, "GET", referer);
                pills = JsonConvert.DeserializeObject<IEnumerable<Pill>>(sitePills);
            }
            return pills;
        }

        public static IEnumerable<Pill> GetPillsByMultipleParametres(UserRequest request)
        {
            IEnumerable<Pill> pills = Enumerable.Empty<Pill>();
            if (request != null)
            {
                string pillsUrl = $"https://api.webpoisoncontrol.org/api/pillid/?a={request.PillFrontSideId}&b={request.PillBackSideId}&colors={request.PillColors}&shapes={request.PillShape}&q={request.PillName}";
                var sitePills = SiteParser.Parser.SendRequest(pillsUrl, "GET", referer);
                pills = JsonConvert.DeserializeObject<IEnumerable<Pill>>(sitePills);
            }
            return pills;
        }

        private static PillSearch GetPillSearchResultByName(string name)
        {
            PillSearch pillSearch = null;
            if (name != null)
            {
                string pillsUrl = $"https://api.webpoisoncontrol.org/api/pillsearch/?search={name}";
                var sitePills = SiteParser.Parser.SendRequest(pillsUrl, "GET", referer);
                pillSearch = JsonConvert.DeserializeObject<PillSearch>(sitePills);
            }
            return pillSearch;
        }

        private static bool isAnyProductsReturned(PillSearch entity)
        {
            bool anyProducts = false;
            if (entity.Products != null)
                anyProducts = true;
            return anyProducts;
        }
        
        private static bool isAnyGroupsReturned(PillSearch entity)
        {
            bool anyGroups = false;
            if (entity.Groups != null)
                anyGroups = true;
            return anyGroups;
        }

    }
}