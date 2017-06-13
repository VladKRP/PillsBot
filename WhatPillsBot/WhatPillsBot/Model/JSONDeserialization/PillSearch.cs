using System;
using System.Collections.Generic;

namespace WhatPillsBot.Model.JSONDeserialization
{
    [Serializable]
    public class PillSearch
    {
        public IEnumerable<PillGroup> Groups { get; set; }

        public IEnumerable<Pill> Products { get; set; }
    }
}