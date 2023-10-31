using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Configuration;
using System.Net;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Security.Policy;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json.Linq;
using System.Diagnostics;



namespace TestProject_Inn
{
    class Program
    {  
                
        private static ITelegramBotClient _botClient;       
        private static ReceiverOptions _receiverOptions;
        
        static async Task Main()
        {
            string Api_Telegram = ConfigurationManager.AppSettings["API_telegram"];
            _botClient = new TelegramBotClient(Api_Telegram); 
            _receiverOptions = new ReceiverOptions 
            {
                AllowedUpdates = new[] 
                {
                UpdateType.Message, 
                UpdateType.CallbackQuery 
            },

                
                ThrowPendingUpdates = true,
            };

            var cts = new CancellationTokenSource();

         
            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); 

            var me = await _botClient.GetMeAsync(); 
            Console.WriteLine($"{me.FirstName} запущен!");

            await Task.Delay(-1); 
        }
        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            bool waitInn = false;
            try
            {

                switch (update.Type)
                {
                    case UpdateType.Message:
                        {

                            var message = update.Message;
                            var user = message.From;
                            Console.WriteLine($"{user.FirstName} ({user.Id}) написал сообщение: {message.Text}");

                            var chat = message.Chat;

                            if (!waitInn)
                            {
                                switch (message.Type)
                                {

                                    case MessageType.Text:
                                        {

                                            if (message.Text == "/start")
                                            {
                                                await botClient.SendTextMessageAsync(
                                                    chat.Id,
                                                    "Добрый день готовы начать?\n" +
                                                    "/reply\n");
                                                return;
                                            }

                                            if (message.Text == "/reply")
                                            {

                                                var replyKeyboard = new ReplyKeyboardMarkup(
                                                    new List<KeyboardButton[]>()
                                                    {
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("/help"),
                                            new KeyboardButton("/hello"),
                                        },
                                        new KeyboardButton[]
                                        {
                                            new KeyboardButton("/inn"),

                                            new KeyboardButton("/full"),
                                            new KeyboardButton("/egrul")
                                        }
                                                    })
                                                {
                                                    ResizeKeyboard = true,
                                                };

                                                await botClient.SendTextMessageAsync(
                                                    chat.Id,
                                                    "",
                                                    replyMarkup: replyKeyboard);

                                                return;
                                            }

                                            if (message.Text == "/help")
                                            {
                                                await botClient.SendTextMessageAsync(
                                                    chat.Id,
                                                    " /start – начать общение с ботом.\r\n" +
                                                    " /help  – вывести справку о доступных командах.\r\n" +
                                                    " /hello – вывести ваше имя и фамилию, ваш email, и дату получения задания.\r\n" +
                                                    " /inn   – получить наименования и адреса компаний по ИНН. Предусмотреть возможность указания нескольких ИНН за одно обращение к боту./full – по ИНН выводить подробную информацию об одной компании, включая список учредителей.\r\n" +
                                                    " /full  – по ИНН выводить подробную информацию об одной компании, включая список учредителей.\r\n" +
                                                    " /egrul - по ИНН компании выдаёт pdf-файл с выпиской из ЕГРЮЛ.",
                                                    replyToMessageId: message.MessageId);

                                                return;
                                            }
                                            if (message.Text == "/full")
                                            {
                                                await botClient.SendTextMessageAsync(
                                                    chat.Id,
                                                    " Пока не работает",
                                                    replyToMessageId: message.MessageId);

                                                return;
                                            }
                                            if (message.Text == "/egrul")
                                            {
                                                await botClient.SendTextMessageAsync(
                                                    chat.Id,
                                                    " Пока не работает",
                                                    replyToMessageId: message.MessageId);

                                                return;
                                            }


                                            if (message.Text == "/inn")
                                            {
                                                waitInn = true;
                                                await botClient.SendTextMessageAsync(
                                                   chat.Id,
                                                   "Введите ИНН",
                                                   replyToMessageId: message.MessageId);

                                                return;
                                            }


                                            if (message.Text == "/hello")
                                            {

                                                await botClient.SendTextMessageAsync(
                                                chat.Id,
                                                "Георгий Чернуха, mr.morg.ru@gmail.com, 30.10.2023",
                                                replyToMessageId: message.MessageId);

                                                return;
                                            }

                                            return;
                                        }






                                    default:
                                        {
                                            await botClient.SendTextMessageAsync(
                                                chat.Id,
                                                "Используйте только текст!");
                                            return;
                                        }
                                }


                            }
                        
                            else
                        {
                                if (message.Text.Length==10)
                                {
                                    waitInn= false;
                                    var INN = message.Text;
                                    string Api_Inn = ConfigurationManager.AppSettings["API_inn"];
                                    var url = "https://api-fns.ru/api/search?q=" + INN + "&key=" + Api_Inn;

                                    var parsed = JsonConvert.DeserializeObject(new WebClient { Encoding = Encoding.UTF8 }.DownloadString(url));


                                    var json = JObject.Parse(parsed.ToString());

                                    var items = json["items"];



                                    await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    "имя компании: " + items[0]["ЮЛ"]["НаимСокрЮЛ"].ToString() + "\r\n адрес компании: " + items[0]["ЮЛ"]["АдресПолн"].ToString(),
                                    replyToMessageId: message.MessageId);

                                    return;
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(
                                               chat.Id,
                                               "Впишите ИНН(10 цифр)");
                                    return;
                                }
                                
                            }
                }    

                    case UpdateType.CallbackQuery:
                        {
                           
                            var callbackQuery = update.CallbackQuery;

                           
                            var user = callbackQuery.From;

                            
                           
                            var chat = callbackQuery.Message.Chat;

                           
                            switch (callbackQuery.Data)
                            {
                               
                            }

                            return;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            
            return Task.CompletedTask;
        }          
    }
}