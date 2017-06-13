using Newtonsoft.Json;
using System.Collections.Generic;

namespace WhatPillsBot.Model.JSONDeserialization
{

    [System.Serializable]
    public class Pill{

        [JsonProperty("PillId")]
        public string Id{get;set;}

        public string Name{get;set;}

        public string ImageUrl{get;set;}

        public string GroupName{get;set;}

        public string Imprint{get;set;}

        public string Shape{get;set;}

        public string Usage{get;set;}

        public IEnumerable<string> Colors{get;set;}
        
        public IEnumerable<Ingredient> Ingredients {get;set;}

    }
}