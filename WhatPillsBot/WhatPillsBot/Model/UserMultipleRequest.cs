
namespace WhatPillsBot.Model
{
    [System.Serializable]
    public class UserMultipleRequest
    {
        public string PillFrontSideId { get; set; }
        public string PillBackSideId { get; set; }
        public string PillShape { get; set; }
        public string PillColors { get; set; }

        public UserMultipleRequest()
        {
            PillFrontSideId = "";
            PillBackSideId = "";
            PillShape = "";
            PillColors = "";
        }
    }
}