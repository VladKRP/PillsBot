using Newtonsoft.Json;

namespace WhatPillsBot.Model
{
    [System.Serializable]
    public class Ingredient{

        public string Id{get;set;}
        public string Name{get;set;}
        public string Strength{get;set;}
        public string StrengthUOM{get;set;}
    }

}
