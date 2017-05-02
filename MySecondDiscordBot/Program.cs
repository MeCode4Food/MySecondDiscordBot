using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace MySecondDiscordBot
{
    public class Program
    {
        static void Main(string[] args) =>
            new Program().Start().GetAwaiter().GetResult();

        private CommandService commands;
        private DiscordSocketClient client;
        private CommandHandler handler;

        private async Task Start()
        {
            //Instatiate WebSocket client
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance
            });

            var token = "MzA0MzMxMzg4MjU1OTkzODU4.C-jiTQ.gcTHVLOEa5stdoXRABLEFGuJTGU";

            client.Log += Log;

            //Connect to discord
            await client.LoginAsync(TokenType.Bot, token, true);
            await client.StartAsync();

            var map = new DependencyMap();
            map.Add(client);

            handler = new CommandHandler();
            await handler.Install(map);

            //Block this program until this is closed;
            await Task.Delay(-1);

        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
