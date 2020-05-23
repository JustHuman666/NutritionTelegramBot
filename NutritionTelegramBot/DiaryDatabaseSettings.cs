using System;
using System.Collections.Generic;
using System.Text;

namespace NutritionTelegramBot
{
    public class DiaryDatabaseSettings : IDiaryDatabaseSettings
    {
        public string CollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IDiaryDatabaseSettings
    {
        string CollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
