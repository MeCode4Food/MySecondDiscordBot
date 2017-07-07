using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

namespace MySecondDiscordBot
{
    public class Program
    {
        static void Main(string[] args)
        {

            handler1 = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler1, true);
            new Program().Start().GetAwaiter().GetResult();

        }

        //handles console closing
        #region 
        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2) //event type 2 is... closing the window?
            {
                Console.WriteLine("Console window closing, death imminent");

                Database database = new Database("discord");
                StringBuilder sb = new StringBuilder();

                sb.Append(string.Format("UPDATE usersession SET session_end = '{0}', force_end = '1'  WHERE force_end = ''"));

                Console.WriteLine(string.Format("Executing Command Cleanup"));

                try
                {
                    database.ExecuteQuery(sb.ToString());
                }
                catch
                {
                    Console.WriteLine(string.Format("Error: Command Cleanup Failed"));
                }
            }
            return false;
        }
        static ConsoleEventDelegate handler1;   // Keeps it from getting garbage collected
        
        // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        #endregion



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
            })
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
