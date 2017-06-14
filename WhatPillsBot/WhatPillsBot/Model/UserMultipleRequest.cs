
namespace WhatPillsBot.Model
{
    [System.Serializable]
    public class UserMultipleRequest
    {
        public string PillName { get; set; }
        public string PillGroup { get; set; }
        public string PillFrontSideId { get; set; }
        public string PillBackSideId { get; set; }
        public string PillShape { get; set; }
        public string PillColors { get; set; }

        public UserMultipleRequest()
        {
            PillName = "";
            PillGroup = "";
            PillFrontSideId = "";
            PillBackSideId = "";
            PillShape = "";
            PillColors = "";
        }
    }
}