using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using WhatPillsBot.Model.JSONDeserialization;
using WhatPillsBot.Model;

namespace WhatPillsBot.Services
{
    public class PillsChecker
    {
        protected readonly string referer = "https://pill-id.webpoisoncontrol.org/";
        protected readonly string siteUrl = "https://api.webpoisoncontrol.org";

        public Pill GetPill(string id)
        {
            string pillUrl = $"{siteUrl}/api/pill/{id}?caseId=1";
            var sitePill = SiteParser.Parser.SendRequest(pillUrl, "GET", referer);
            var pill = JsonConvert.DeserializeObject<Pill>(sitePill);
            pill.Usage = pill.Usage.Replace("<p>", "").Replace("</p>", "");
            return pill;
        }

        public string GetPillUsage(string id)
        {
            string pillUrl = $"{siteUrl}/api/pill/{id}?caseId=1";
            var sitePill = SiteParser.Parser.SendRequest(pillUrl, "GET", referer);
            var pill = JsonConvert.DeserializeObject<Pill>(sitePill);
            return pill.Usage.Replace("<p>","").Replace("</p>","");
        }

        public IEnumerable<PillGroup> GetPillGroups(string name)
        {
            IEnumerable<PillGroup> groups = Enumerable.Empty<PillGroup>();
            var pillSearch = GetPillSearchResultByName(name);
            if (isAnyGroupsReturned(pillSearch))
                groups = pillSearch.Groups;
            return groups;
        }

        public IEnumerable<Pill> GetPillProducts(string name)
        {
            IEnumerable<Pill> pills = Enumerable.Empty<Pill>();
            var pillSearch = GetPillSearchResultByName(name);
            if (isAnyProductsReturned(pillSearch))
               pills = pillSearch.Products;
            return pills;
        }

        public IEnumerable<Pill> GetPillByGroupIdAndName(string id, string name)
        {
            IEnumerable<Pill> pills = Enumerable.Empty<Pill>();
            if(!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name))
            {
                string pillsUrl = $"{siteUrl}/api/pillgroups?id={id}&search={name}";
                var sitePills = SiteParser.Parser.SendRequest(pillsUrl, "GET", referer);
                pills = JsonConvert.DeserializeObject<IEnumerable<Pill>>(sitePills);
            }
            return pills;
        }

        public IEnumerable<Pill> GetPillsByMultipleParametres(UserMultiplePillRequest request)
        {
            IEnumerable<Pill> pills = Enumerable.Empty<Pill>();
            if (request != null)
            {
                string pillsUrl = $"{siteUrl}/api/pillid/?a={request.FrontSideId}&b={request.BackSideId}&colors={request.Colors}&shapes={request.Shape}";
                var sitePills = SiteParser.Parser.SendRequest(pillsUrl, "GET", referer);
                pills = JsonConvert.DeserializeObject<IEnumerable<Pill>>(sitePills);
            }
            return pills;
        }

        private PillSearch GetPillSearchResultByName(string name)
        {
            PillSearch pillSearch = null;
            if (name != null)
            {
                string pillsUrl = $"{siteUrl}/api/pillsearch/?search={name}";
                var sitePills = SiteParser.Parser.SendRequest(pillsUrl, "GET", referer);
                pillSearch = JsonConvert.DeserializeObject<PillSearch>(sitePills);
            }
            return pillSearch;
        }

        private bool isAnyProductsReturned(PillSearch entity)
        {
            bool anyProducts = false;
            if (entity.Products != null)
                anyProducts = true;
            return anyProducts;
        }
        
        private bool isAnyGroupsReturned(PillSearch entity)
        {
            bool anyGroups = false;
            if (entity.Groups != null)
                anyGroups = true;
            return anyGroups;
        }
    }
}