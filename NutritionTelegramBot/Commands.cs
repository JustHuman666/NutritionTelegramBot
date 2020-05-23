using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NutritionTelegramBot
{
    public class Commands
    {
        public IMongoCollection<UserDb> userInfo;
        public Commands()
        {
            var client = new MongoClient("mongodb+srv://Eleonora:Eleonora@cluster0-nkuob.azure.mongodb.net/test?retryWrites=true&w=majority");
            var database = client.GetDatabase("CaloriesDiary");
            userInfo = database.GetCollection<UserDb>("UserInfo");
        }
        public UserDb Get(string id) =>
            userInfo.Find<UserDb>(user => user.id == id).FirstOrDefault();

        public async Task<UserDb> Create(UserDb user)
        {
            await userInfo.InsertOneAsync(user);
            return user;
        }

        public async void UpdateAsync(string id, UserDb userIn) =>
            await userInfo.ReplaceOneAsync(user => user.id == id, userIn);

    }
}
