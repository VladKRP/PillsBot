using System.Collections.Generic;

namespace WhatPillsBot.Model
{
    [System.Serializable]
    public class Pill
    {
        public string Name { get; set; }
        public string FrontSideId { get; set; }
        public string BackSideId { get; set; }
        public string Shape { get; set; }
        public IEnumerable<string> Colors { get; set; }
    }
}