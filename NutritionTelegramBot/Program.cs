using System;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace NutritionTelegramBot
{
    class Program
    {
        static TelegramBotClient Bot;

        static void Main()
        {
            
            Bot = new TelegramBotClient("1165129304:AAGl-SvEQ12HblgefVJJOAZ0vno53wiaMuY") { Timeout = TimeSpan.FromSeconds(10)};
            var me = Bot.GetMeAsync().Result;
            Bot.OnMessage += botOnMessage;
            Bot.OnCallbackQuery += botOnCallbackQuery;
            Console.WriteLine(me.FirstName);
            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static void botOnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static async void botOnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            if (message == null || message.Type != MessageType.Text) 
            {
                return;
            }
            string name = $"{ message.From.FirstName}";
            string id = $"{message.From.Id}";
            Console.WriteLine($"'{name}' with id '{id}' sent message: '{message.Text}'");
            string startText = @"Welcome. That bot can do some interesting things. Here are the list of commands: 
/start - you can start work;
/findbyname - you can find information about food nutrition by writing its name, bot will show you information for 1 portion. In case when bot can't find information, but you think that it should be, try to write name with some adjectives, like 'big banana';
/findrecipe - you can find recipes by writing their name or you can find recipes of dishes with special products, bot will show you 5 recipes, for example you can write 'rainbow ice cream' and enjoy the result;
/addfavourite - if you eat some products more than one time at month you can add information about them for faster getting information, just write the name of product or dish and send it, if bot can't find information, but you think that it should be, try to write name with some adjectives, like 'big banana';
/getfavourite - if you want to get the list of your notes just use this command;
/deletefromfavourite - if you want to delete some notes from your list of favourites use this command, you just need to send the number of note that you want to delete, if you don't know this number, firstly use command '/getfavourite'.";
            string noCommand = "This bot does not have such command";
            using var client = new HttpClient();
            Commands command = new Commands();
            User user = new User();
            user.Id = message.From.Id.ToString();
            UserDb userdb = new UserDb();
            try
            {
                switch (message.Text)
                {
                    case "/start":

                        if (command.Get(user.Id) == null)
                        {
                            userdb.id = user.Id;
                            userdb.status = 1;
                            await command.Create(userdb);
                            var content3 = await client.GetStringAsync($"https://caloriesapi.azurewebsites.net/api/Cal/{user.Id}");
                            User user3 = JsonConvert.DeserializeObject<User>(content3);
                            if (user3 == null)
                            {
                                RequestInfo request = new RequestInfo();
                                request.id = user.Id;
                                request.Food = "";
                                var json = JsonConvert.SerializeObject(request);
                                var data = new StringContent(json, Encoding.UTF8, "application/json");
                                var response = await client.PostAsync("https://caloriesapi.azurewebsites.net/api/Cal/create", data);
                            }
                        }
                        userdb = command.Get(user.Id);
                        userdb.id = user.Id;
                        userdb.status = 1;
                        command.UpdateAsync(userdb.id, userdb);
                        await Bot.SendPhotoAsync(message.From.Id, "https://cdn-ep19.pressidium.com/wp-content/uploads/2019/01/food-photography-blogs-spread.jpg");
                        await Bot.SendTextMessageAsync(message.From.Id, startText);
                        break;

                    case "/findbyname":
                        UserDb userbyname = new UserDb();
                        if (command.Get(user.Id) == null)
                        {
                            userbyname.id = user.Id;
                            userbyname.status = 1;
                            await command.Create(userbyname);
                        }
                        userbyname.id = user.Id;
                        userbyname.status = 2;
                        command.UpdateAsync(userbyname.id, userbyname);
                        string requestforname = "Write the name of product or dish.";
                        await Bot.SendTextMessageAsync(message.From.Id, requestforname);
                        break;

                    case "/findrecipe":
                        UserDb userforrecipe = new UserDb();
                        if (command.Get(user.Id) == null)
                        {
                            userforrecipe.id = user.Id;
                            userforrecipe.status = 1;
                            await command.Create(userforrecipe);
                        }
                        userforrecipe.id = user.Id;
                        userforrecipe.status = 3;
                        command.UpdateAsync(userforrecipe.id, userforrecipe);
                        string requestforrecipe = "Write the name of dish or product in the recipe you want to find.";
                        await Bot.SendTextMessageAsync(message.From.Id, requestforrecipe);
                        break;

                    case "/addfavourite":
                        UserDb useradd = new UserDb();
                        if (command.Get(user.Id) == null)
                        {
                            useradd.id = user.Id;
                            useradd.status = 1;
                            await command.Create(useradd);
                        }
                        useradd.id = user.Id;
                        var content2 = await client.GetStringAsync($"https://caloriesapi.azurewebsites.net/api/Cal/{user.Id}");
                        User user2 = JsonConvert.DeserializeObject<User>(content2);
                        if (user2 != null)
                        {
                            useradd.status = 4;
                            command.UpdateAsync(useradd.id, useradd);
                            string requestforadd = "Write the name of product that you'd like to add to favourite.";
                            await Bot.SendTextMessageAsync(message.From.Id, requestforadd);
                        }
                        else
                        {
                            await Bot.SendTextMessageAsync(message.From.Id, "Firstly, choose the start command and continue using of this bot.");
                        }
                        break;

                    case "/getfavourite":
                        user.Id = Convert.ToString(e.Message.From.Id);
                        var content = await client.GetStringAsync($"https://caloriesapi.azurewebsites.net/api/Cal/{user.Id}");
                        User user1 = JsonConvert.DeserializeObject<User>(content);
                        string result = "";
                        int n = 1;
                        if (user1 != null)
                        {
                            if (user1.food[0].foodName == "")
                            {
                                user1.food.RemoveAt(0);
                            }
                            foreach (FoodInfo food in user1.food)
                            {
                                string res = @$" 
{n})  {food.foodAmount} {food.foodName} - {food.totalWeight:F2} g
{food.totalNutrients.cHOCDF.label} : {food.totalNutrients.cHOCDF.quantity:F2} {food.totalNutrients.cHOCDF.unit}
{food.totalNutrients.eNERC_KCAL.label} : {food.totalNutrients.eNERC_KCAL.quantity:F2} {food.totalNutrients.eNERC_KCAL.unit}
{food.totalNutrients.fat.label} : {food.totalNutrients.fat.quantity:F2} {food.totalNutrients.fat.unit}
{food.totalNutrients.pROCNT.label} : {food.totalNutrients.pROCNT.quantity:F2} {food.totalNutrients.pROCNT.unit} 

                        ";
                                result = result.Insert(result.Length, res);
                                n++;
                            }
                            if (user1.food.Count == 0)
                            {
                                await Bot.SendTextMessageAsync(message.From.Id, "You don't have any favourites yet.");
                            }
                            else
                            {
                                await Bot.SendTextMessageAsync(message.From.Id, "Here is the list of your favourites.");
                                await Bot.SendTextMessageAsync(message.From.Id, result);
                            }
                           
                        }
                        else
                        {
                            await Bot.SendTextMessageAsync(e.Message.From.Id, "You don't have any notes in 'favourites'.");
                        }
                        break;

                    case "/deletefromfavourite":
                        UserDb userfordelete = new UserDb();
                        if (command.Get(user.Id) == null)
                        {
                            userfordelete.id = user.Id;
                            userfordelete.status = 1;
                            await command.Create(userfordelete);
                        }
                        userfordelete.id = user.Id;
                        userfordelete.status = 5;
                        command.UpdateAsync(userfordelete.id, userfordelete);
                        var content4 = await client.GetStringAsync($"https://caloriesapi.azurewebsites.net/api/Cal/{user.Id}");
                        User user4 = JsonConvert.DeserializeObject<User>(content4);
                        if (user4 != null)
                        {
                            userfordelete.status = 5;
                            command.UpdateAsync(userfordelete.id, userfordelete);
                            string requestfordelete = "Write the number of note that you'd like to delete.";
                            await Bot.SendTextMessageAsync(message.From.Id, requestfordelete);
                        }
                        else
                        {
                            await Bot.SendTextMessageAsync(message.From.Id, "Firstly, choose the start command and continue using of this bot.");
                        }
                        
                        break;

                    default:

                        userdb = command.Get(user.Id);
                        if (userdb == null)
                        {
                            userdb.id = user.Id;
                            userdb.status = 1;
                            await command.Create(userdb);
                            await Bot.SendTextMessageAsync(message.From.Id, "Please try again.");
                        }
                        
                        else
                        {
                            switch (userdb.status)
                            {
                                case 0:
                                    var userreturn0 = command.Get(user.Id);
                                    userreturn0.id = user.Id;
                                    userreturn0.status = 1;
                                    command.UpdateAsync(userreturn0.id, userreturn0);
                                    await Bot.SendTextMessageAsync(message.From.Id, noCommand);
                                    break;

                                case 1:
                                    var userreturns = command.Get(user.Id);
                                    userreturns.id = user.Id;
                                    userreturns.status = 0;
                                    command.UpdateAsync(userreturns.id, userreturns);
                                    await Bot.SendTextMessageAsync(message.From.Id, "Choose the command you need.");
                                    break;

                                case 2:
                                    var userreturn = command.Get(user.Id);
                                    userreturn.id = user.Id;
                                    userreturn.status = 1;
                                    command.UpdateAsync(userreturn.id, userreturn);
                                    RequestInfo requestInfo = new RequestInfo();
                                    requestInfo.Food = message.Text;
                                    var json = JsonConvert.SerializeObject(requestInfo);
                                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                                    var response = await client.PostAsync("https://caloriesapi.azurewebsites.net/api/Cal/nutrition", data);
                                    string res = response.Content.ReadAsStringAsync().Result;
                                    FoodInfo foodInfo = new FoodInfo();
                                    foodInfo = JsonConvert.DeserializeObject<FoodInfo>(res);
                                    if (foodInfo.totalWeight == 0)
                                    {
                                        await Bot.SendTextMessageAsync(message.From.Id, "Bot can't find information about this product, try again.");
                                    }
                                    else
                                    {
                                        string resultresponse = @$" 
{foodInfo.foodAmount} {foodInfo.foodName} - {foodInfo.totalWeight:F2} g
{foodInfo.totalNutrients.cHOCDF.label} : {foodInfo.totalNutrients.cHOCDF.quantity:F2} {foodInfo.totalNutrients.cHOCDF.unit}
{foodInfo.totalNutrients.eNERC_KCAL.label} : {foodInfo.totalNutrients.eNERC_KCAL.quantity:F2} {foodInfo.totalNutrients.eNERC_KCAL.unit}
{foodInfo.totalNutrients.fat.label} : {foodInfo.totalNutrients.fat.quantity:F2} {foodInfo.totalNutrients.fat.unit}
{foodInfo.totalNutrients.pROCNT.label} : {foodInfo.totalNutrients.pROCNT.quantity:F2} {foodInfo.totalNutrients.pROCNT.unit} 

                        ";
                                        await Bot.SendTextMessageAsync(message.From.Id, "Here is the nutrition of this product, that we found.");
                                        await Bot.SendTextMessageAsync(message.From.Id, resultresponse);
                                    }

                                    break;

                                case 3:
                                    var userreturn1 = command.Get(user.Id);
                                    userreturn1.id = user.Id;
                                    userreturn1.status = 1;
                                    command.UpdateAsync(userreturn1.id, userreturn1);
                                    RequestInfo requestInfo1 = new RequestInfo();
                                    requestInfo1.Recipe = message.Text;
                                    var json1 = JsonConvert.SerializeObject(requestInfo1);
                                    var data1 = new StringContent(json1, Encoding.UTF8, "application/json");
                                    var response1 = await client.PostAsync("https://caloriesapi.azurewebsites.net/api/Cal/recipe", data1);
                                    string res1 = response1.Content.ReadAsStringAsync().Result;
                                    RecipeInfo recipeInfo = new RecipeInfo();
                                    recipeInfo = JsonConvert.DeserializeObject<RecipeInfo>(res1);
                                    int m = 1;
                                    if (recipeInfo.hits.Count != 0)
                                    {
                                        await Bot.SendTextMessageAsync(message.From.Id, "Here is the recipes with nutrition of dishes, that we found.");
                                        foreach (Recipes recipe in recipeInfo.hits)
                                        {
                                            string ingredients = "";
                                            foreach (string ingredient in recipe.recipe.ingredientLines)
                                            {
                                                ingredients = ingredients.Insert(ingredients.Length, ingredient + "\n");
                                            }
                                            string resresponse = @$"
{m}){recipe.recipe.image}
The link of recipe with detail instructions:
{recipe.recipe.shareAs}

Ingredients: 

{ingredients}
Total time of cooking: {recipe.recipe.totalTime};

Total weight of cooked dishes: {recipe.recipe.totalWeight:F2};

Total nutrients of dish: 

{recipe.recipe.totalNutrients.cHOCDF.label} : {recipe.recipe.totalNutrients.cHOCDF.quantity:F2} {recipe.recipe.totalNutrients.cHOCDF.unit}
{recipe.recipe.totalNutrients.eNERC_KCAL.label} : {recipe.recipe.totalNutrients.eNERC_KCAL.quantity:F2} {recipe.recipe.totalNutrients.eNERC_KCAL.unit}
{recipe.recipe.totalNutrients.fat.label} : {recipe.recipe.totalNutrients.fat.quantity:F2} {recipe.recipe.totalNutrients.fat.unit}
{recipe.recipe.totalNutrients.pROCNT.label} : {recipe.recipe.totalNutrients.pROCNT.quantity:F2} {recipe.recipe.totalNutrients.pROCNT.unit} 


";
                                            await Bot.SendTextMessageAsync(message.From.Id, resresponse);
                                            m++;
                                        }
                                    }
                                    else
                                    {
                                        await Bot.SendTextMessageAsync(message.From.Id, "Bot can't find any recipe with this name or product, try again.");
                                    }
                                    break;

                                case 4:
                                    var userreturn2 = command.Get(user.Id);
                                    userreturn2.id = user.Id;
                                    userreturn2.status = 1;
                                    command.UpdateAsync(userreturn2.id, userreturn2);
                                    RequestInfo requestInfo2 = new RequestInfo();
                                    requestInfo2.Food = message.Text;
                                    requestInfo2.id = user.Id;
                                    var json2 = JsonConvert.SerializeObject(requestInfo2);
                                    var data2 = new StringContent(json2, Encoding.UTF8, "application/json");
                                    var response3 = await client.PostAsync("https://caloriesapi.azurewebsites.net/api/Cal/nutrition", data2);
                                    string result3 = response3.Content.ReadAsStringAsync().Result;
                                    FoodInfo foodInfo3 = new FoodInfo();
                                    foodInfo = JsonConvert.DeserializeObject<FoodInfo>(result3);
                                    if (foodInfo.totalWeight == 0)
                                    {
                                        await Bot.SendTextMessageAsync(message.From.Id, "Bot can't find information about this product, try again.");

                                    }
                                    else
                                    {
                                        var response2 = await client.PutAsync("https://caloriesapi.azurewebsites.net/api/Cal/update", data2);
                                        await Bot.SendTextMessageAsync(message.From.Id, "Note was added to favourites.");
                                    }
                                    break;

                                case 5:
                                    var userreturn3 = command.Get(user.Id);
                                    userreturn3.id = user.Id;
                                    userreturn3.status = 1;
                                    command.UpdateAsync(userreturn3.id, userreturn3);
                                    var content5 = await client.GetStringAsync($"https://caloriesapi.azurewebsites.net/api/Cal/{user.Id}");
                                    User user5 = JsonConvert.DeserializeObject<User>(content5);
                                    if (user5 != null)
                                    {
                                        if (user5.food.Count > 1)
                                        {

                                            RequestInfo request5 = new RequestInfo();
                                            request5.id = user.Id;
                                            try
                                            {
                                                request5.numberNote = Convert.ToInt32(message.Text);
                                            }
                                            catch (FormatException)
                                            {
                                                await Bot.SendTextMessageAsync(message.From.Id, "Incorrect format, please write numbers but not letters or words. Try again.");
                                                break;
                                            }
                                            if (request5.numberNote <= user5.food.Count)
                                            {
                                                var json5 = JsonConvert.SerializeObject(request5);
                                                var data5 = new StringContent(json5, Encoding.UTF8, "application/json");
                                                var response5 = await client.PutAsync("https://caloriesapi.azurewebsites.net/api/Cal/delete", data5);
                                                await Bot.SendTextMessageAsync(message.From.Id, "The note was deleted.");
                                            }

                                            else
                                            {
                                                await Bot.SendTextMessageAsync(message.From.Id, "Nothing to delete, try with another number.");
                                            }

                                        }
                                        else
                                        {
                                            await Bot.SendTextMessageAsync(message.From.Id, "You don't have any notes in favourites.");
                                        }
                                    }
                                    else
                                    {
                                        await Bot.SendTextMessageAsync(message.From.Id, "Firstly, choose the start command and continue using of this bot.");
                                    }
                                    break;

                                default:
                                    await Bot.SendTextMessageAsync(message.From.Id, noCommand);
                                    break;

                            }
                            break;
                        }
                        break;
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {

            }
        }
    }
}
