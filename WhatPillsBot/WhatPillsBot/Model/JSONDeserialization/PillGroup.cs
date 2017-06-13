using System;

namespace WhatPillsBot.Model.JSONDeserialization
{
    [Serializable]
    public class PillGroup
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Group { get; set; }

    }
}