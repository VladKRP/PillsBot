using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhatPillsBot.Model
{
    public class Pill
    {
        public string Name { get; set; }
        public string FrontSideId { get; set; }
        public string BackSideId { get; set; }
        public string Shape { get; set; }
        public IEnumerable<string> Color { get; set; }
    }
}