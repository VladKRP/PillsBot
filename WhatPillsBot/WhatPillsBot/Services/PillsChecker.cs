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

        public static IEnumerable<Pill> GetPillsByName(string name)
        {
            IEnumerable<Pill> pills = Enumerable.Empty<Pill>();
            if(name != null)
            {
                string pillsUrl = $"https://api.webpoisoncontrol.org/api/pillsearch/?search={name}";
                var sitePills = SiteParser.Parser.SendRequest(pillsUrl, "GET", referer);
                pills = JsonConvert.DeserializeObject<PillSearch>(sitePills).Products;           
            }
            return pills;
        }   
        
        public static void GetPillsOrGroupsByName()
        {

        }

        public static IEnumerable<Group> GetPillGroups(UserRequest request)
        {
            return null;
        }

        public static IEnumerable<Pill> GetPillProducts(UserRequest request)
        {
            return null;
        }

        public static bool isAnyProductsReturned(PillSearch entity)
        {
            bool anyProducts = false;
            if (entity.Products != null)
                anyProducts = true;
            return anyProducts;
        }
        
        public static bool isAnyGroupsReturned(PillSearch entity)
        {
            bool anyGroups = false;
            if (entity.Products != null)
                anyGroups = true;
            return anyGroups;
        }

        public static IEnumerable<Pill> GetPillsByMultipleParametres(UserRequest request)
        {
            IEnumerable<Pill> pills = Enumerable.Empty<Pill>();
            if(request != null)
            {
                string pillsUrl = $"https://api.webpoisoncontrol.org/api/pillid/?a={request.FrontSideId}&b={request.BackSideId}&colors={request.Colors}&shapes={request.Shape}&q={request.Name}";
                var sitePills = SiteParser.Parser.SendRequest(pillsUrl, "GET", referer);
                pills = JsonConvert.DeserializeObject<IEnumerable<Pill>>(sitePills);
            }
            return pills;
        }

    }
}