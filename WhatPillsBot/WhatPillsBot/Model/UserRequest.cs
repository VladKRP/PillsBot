
namespace WhatPillsBot.Model
{
    [System.Serializable]
    public class UserRequest
    {
        public string PillName { get; set; }
        public string PillFrontSideId { get; set; }
        public string PillBackSideId { get; set; }
        public string PillShape { get; set; }
        public string PillColors { get; set; }
        public string PillGroup { get; set; }

        public UserRequest()
        {
            PillName = "";
            PillFrontSideId = "";
            PillBackSideId = "";
            PillShape = "";
            PillColors = "";
            PillGroup = "";
        }
    }
}