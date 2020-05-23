using System;
using System.Collections.Generic;
using System.Text;

namespace NutritionTelegramBot
{
    public class FoodInfo
    {
        public string foodName { get; set; }
        public double foodAmount = 1;
        public float totalWeight { get; set; }

        public totalNutrients totalNutrients { get; set; }
    }
}
