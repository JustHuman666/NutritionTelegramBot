using System;
using System.Collections.Generic;
using System.Text;

namespace NutritionTelegramBot
{
    public class User
    {
        public string Id { get; set; }
        public List<FoodInfo> food = new List<FoodInfo>();

    }
}
