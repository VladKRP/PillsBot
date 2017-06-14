using System;
using System.Collections.Generic;
using System.Linq;

namespace WhatPillsBot.Extensions
{
    public static class UserAgreement
    {
        public static bool isUserAgree(string message)
        {
            var agreeKeyWords = new List<string>() { "Yes", "Yep", "Ye", "y" };
            var isAgree = agreeKeyWords.Where(x => message.IndexOf(x, StringComparison.CurrentCultureIgnoreCase) >= 0).Count() > 0;
            return isAgree;
        }
    }
}