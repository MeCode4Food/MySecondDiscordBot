using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Runtime.InteropServices;
using System.Text;

namespace MySecondDiscordBot
{
    public class Program
    {
        static void Main(string[] args)
        {
            exitHandler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(exitHandler, true);
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

                
                Console.WriteLine("useractivity Cleanup");
                //Get user online list
                foreach(SocketGuild guild in CommandHandler.pGuildList)
                {
                    var userList = guild.Users;
                    foreach(SocketUser user in userList)
                    {
                        if(user.Status.ToString() != "Offline")
                        {
                            sb.Append(string.Format("INSERT INTO useractivity (user_ign ,session_id , status_before , status_after , game_id , timestamp) SELECT '{0}', session_id, '{1}', '{2}', '{3}', '{4}' FROM usersession WHERE user_id = '{5}' AND session_end = '1900-01-01 00:00:00.000';",
                                user.Username,  user.Status.ToString(), "Console Inactive","", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), user.Id));
                        }
                    }
                }

                Console.WriteLine("usersession Cleanup");
                sb.Append(string.Format("UPDATE usersession SET session_end = '{0}', force_end = '1'  WHERE force_end = '' AND session_end = '1900-01-01 00:00:00.000';", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")));


                //Update data entries for each online user

                Console.WriteLine(string.Format("Executing Command Cleanup"));

                try
                {
                    database.ExecuteQuery(sb.ToString());
                    Console.WriteLine(string.Format("Command Cleanup Success, Shutting Down..."));
                    Task.Delay(200);
                }
                catch
                {
                    Console.WriteLine(string.Format("Error: Command Cleanup Failed"));
                }
            }
            return false;
        }
        static ConsoleEventDelegate exitHandler;   // Keeps it from getting garbage collected
        
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
            

            await Task.Delay(-1);
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
