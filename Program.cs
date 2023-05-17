using Telegram.Bot;
using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace TestChat2
{
    internal class Program
    {
        
        public static ConcurrentDictionary<long, List<Message>> dic = new();
        public static string GPTapiKey = "";
        public static string ApiTG = "";
        public static string GPTendPoint = "https://api.openai.com/v1/chat/completions";
        public static HttpClient httpClient = new HttpClient();//клиент через который идет общение

        public static void SetSettings()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "appSettings.json");
            var json = System.IO.File.ReadAllText(path);
            AppSettings? current = JsonConvert.DeserializeObject<AppSettings>(json);

            GPTapiKey = current.keyGPT;
            ApiTG = current.keyTG;
        }

        static async Task Main(string[] args)
        {
            SetSettings();            

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {GPTapiKey}");

            var botClient = new TelegramBotClient(ApiTG);
            botClient.StartReceiving(Update, Error);
            CancellationTokenSource cts = new();

            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            cts.Cancel();

            Console.ReadKey();
        }

        
        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {

            if (update.Message is not { } message)
                return;
            // проверка на то, что сообщение не пусто
            if (message.Text is not { } messageText)
                return;
            
            var chatId = message.Chat.Id;
            
            Console.WriteLine($"получил сообщение в чате: {chatId}.");

            //--------------------------------------------------------логика chatGPT

            //запоминание сообщений для определенного чата
            if (!dic.ContainsKey(chatId))
            {
                dic[chatId] = new List<Message>();
                var messege = new Message { Role = "user", Content = messageText };
                dic[chatId].Add(messege);
            }
            else
            {
                var messege = new Message { Role = "user", Content = messageText };
                dic[chatId].Add(messege);
            }

            var requestData = new Request
            {
                ModelId = "gpt-3.5-turbo",
                Messages = dic[chatId]
            };

            //отправка запроса / получение запроса, запись его в response
            using var response = await httpClient.PostAsJsonAsync(GPTendPoint, requestData);

            if (!response.IsSuccessStatusCode)
            {
                await botClient.SendTextMessageAsync(chatId,response.StatusCode.ToString());
                Console.WriteLine($"{(int)response.StatusCode} {response.StatusCode}");
                return;
            }

            //парсинг полученного ответа по классу responsedata
            ResponseData? responseData = await response.Content.ReadFromJsonAsync<ResponseData>();

            //создания листа с помощью полученного responsedata 
            var choices = responseData?.Choices ?? new List<Choice>();

            //проверка на наличие не пустого ответа
            if (choices.Count == 0)
            {
                await botClient.SendTextMessageAsync(chatId, "No choices were returned by the API");
                Console.WriteLine("No choices were returned by the API");
                return;
            }
            var choice = choices[0];

            var responseMessage = choice.Message;

            dic[chatId].Add(responseMessage);
            Console.WriteLine($"В чат {chatId}, должен был дать ответ");
            
            
            //-------------------------------- конец логики chatGPT

            await botClient.SendTextMessageAsync(chatId, responseMessage.Content);
            

            //var responseMessege = await botClient.SendTextMessageAsync(chatId: chatId,
            //text: $"Trying *all the parameters* of ```codeblock``` method",
            //parseMode: ParseMode.MarkdownV2,
            //disableNotification: true,
            //replyToMessageId: update.Message.MessageId,
            //cancellationToken: token);

            //нижняя ссылка в сообщении
            //replyMarkup: new InlineKeyboardMarkup(
            //InlineKeyboardButton.WithUrl(
            //text: "Check the sound",
            //url: "https://telegram.org/"))

            //Console.WriteLine(
            //    $"{responseMessege.From.FirstName} sent message {responseMessege.MessageId} " +
            //    $"to chat {responseMessege.Chat.Id} at {responseMessege.Date}. " +
            //    $"It is a reply to message {responseMessege.ReplyToMessage.MessageId} " +
            //    $"and has {responseMessege.Entities.Length} message entities.");
        }
        
        async static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            Console.WriteLine($"{arg2.Message}");
        }

    }
}


