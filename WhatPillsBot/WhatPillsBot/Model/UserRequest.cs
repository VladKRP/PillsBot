
namespace WhatPillsBot.Model
{
    [System.Serializable]
    public class UserRequest
    {
        public string Name { get; set; } = "";
        public string FrontSideId { get; set; } = "";
        public string BackSideId { get; set; } = "";
        public string Shape { get; set; } = "";
        public string Colors { get; set; } = "";
    }
}