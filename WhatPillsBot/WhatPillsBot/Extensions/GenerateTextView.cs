using System.Linq;
using WhatPillsBot.Model.JSONDeserialization;

namespace WhatPillsBot.Extensions
{
    public static class GenerateTextView
    {
        public static string GeneratePillIngredientsString(Pill pill)
        {
            string result = null;
            var ingridients = from x in pill.Ingredients select $"{x.Name}({x.Strength} {x.StrengthUOM})";
            if (ingridients != null && ingridients.Count() > 0)
                result = string.Join("\n", ingridients);
            return result;
        }

        public static string GeneratePillColorsString(Pill pill)
        {
            string result = null;
            if(pill.Colors != null && pill.Colors.Count() > 0)
                result =  string.Join(",", (pill.Colors));
            return result;
        }

    }
}