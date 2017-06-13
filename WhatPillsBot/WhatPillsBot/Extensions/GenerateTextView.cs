using System.Linq;
using WhatPillsBot.Model.JSONDeserialization;

namespace WhatPillsBot.Extensions
{
    public static class GenerateTextView
    {
        public static string GeneratePillIngredientsString(Pill pill)
        {
            var ingredients = from x in pill.Ingredients select $"{x.Name}({x.Strength} {x.StrengthUOM})";
            return string.Join("\n", ingredients);
        }

        public static string GeneratePillColorsString(Pill pill)
        {
            return string.Join(",", (pill.Colors));
        }

    }
}