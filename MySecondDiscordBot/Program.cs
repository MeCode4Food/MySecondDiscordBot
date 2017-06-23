using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.IO;

namespace MySecondDiscordBot
{
    public class Program
    {
        static void Main(string[] args) =>
            new Program().Start().GetAwaiter().GetResult();
        
        private DiscordSocketClient client;
        private CommandHandler handler;

        private async Task Start()
        {
            //Instatiate WebSocket client
            client = new DiscordSocketClient();

            var token = "MzA0MzMxMzg4MjU1OTkzODU4.DCf3kA.50usYahstgxA-ovX1U5T3lVkF8c";

            client.Log += Log;

            //Connect to discord
            await client.LoginAsync(TokenType.Bot, token, true);
            await client.StartAsync();

            var map = new DependencyMap(); //something has been done to help with this error
            map.Add(client);

            handler = new CommandHandler();
            await handler.Install(map);



            ////5 minutes timed function
            //var startTimeSpan = TimeSpan.Zero;
            //var periodTimeSpan = TimeSpan.FromSeconds(10);


            ////var timer = new System.Threading.Timer((e) =>
            ////{
            ////    userOnlineDump();
            ////}, null, startTimeSpan, periodTimeSpan);

            //Block this program until this is closed;
            await Task.Delay(-1);
        }

        private async void userOnlineDump()
        {
            await Task.Run(() =>
            {
                Console.WriteLine("Channels Joined: {0}, {1}", client.Guilds.Count.ToString(), client.Guilds.ToString());
            }
            )
            ;
        }
        private Task UserUpdated(EventArgs e)
        {

            return Task.CompletedTask;
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
