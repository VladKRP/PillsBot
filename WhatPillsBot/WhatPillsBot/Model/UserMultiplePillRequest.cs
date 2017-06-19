
namespace WhatPillsBot.Model
{
    [System.Serializable]
    public class UserMultiplePillRequest
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public string FrontSideId { get; set; }
        public string BackSideId { get; set; }
        public string Shape { get; set; }
        public string Colors { get; set; }

        public UserMultiplePillRequest()
        {
            Name = string.Empty;
            Group = string.Empty;
            FrontSideId = string.Empty;
            BackSideId = string.Empty;
            Shape = string.Empty;
            Colors = string.Empty;
        }
    }
}