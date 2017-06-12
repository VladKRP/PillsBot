using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhatPillsBot.Model
{
    [Serializable]
    public class PillSearch
    {
        public IEnumerable<PillGroup> Groups { get; set; }

        public IEnumerable<Pill> Products { get; set; }
    }
}