using System;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Text;

namespace MySecondDiscordBot
{
    public class CommandHandler
    {
        private CommandService commands;
        private DiscordSocketClient client;
        private IDependencyMap map;

        public async Task Install(IDependencyMap _map)
        {
            //create Command Service, inject it to Dependency map
            client = _map.Get<DiscordSocketClient>();
            commands = new CommandService();

            //_map.Add(commands);

            map = _map;

            //Adds assembly to command service
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            client.MessageReceived += HandleCommand;
            client.UserJoined += AddUser;
            client.GuildMemberUpdated += UserUpdated;
            client.UserLeft += UserLeft;
        }

        private async Task UserLeft(SocketGuildUser user)
        {
            var database = new Database("discord");

            await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(string.Format("UPDATE discorduser SET user_isjoined = '0' WHERE user_id = {0}", user.Id ));

                Console.WriteLine(string.Format("User : {0} , Executing Command UserLeft", user.Username));

                try
                {
                    database.ExecuteQuery(sb.ToString());
                }
                catch
                {
                    Console.WriteLine(string.Format("Error: {0} , Command UserLeft Failed", user.Username));
                }

            });
        }

        private async Task AddUser(SocketGuildUser user)
        {

            var database = new Database("discord");
            
            //This is assuming any method under database class runs the database method first

            await Task.Run(() =>
            {
                
                if(Database.CheckExistingUser(user) == null) //why does this not use an instance of database with argument "discord"? what is static?
                {
                    Console.WriteLine(string.Format("User : {0} , Executing Command AddUser", user.Username));

                    try
                    {
                        Database.EnterUser(user);
                    }
                    catch
                    {
                        Console.WriteLine(string.Format("Error: {0} , Command AddUser Failed", user.Username));
                    }

                    database.CloseConnection();
                }
                else
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(string.Format("UPDATE discorduser SET user_isjoined = '1' WHERE user_id = {0}", user.Id));

                    Console.WriteLine(string.Format("Existing User : {0} , Executing Command AddUser", user.Username));

                    try
                    {
                        database.ExecuteQuery(sb.ToString());
                    }
                    catch
                    {
                        Console.WriteLine(string.Format("Error: {0} , Command Existing AddUser Failed", user.Username));
                    }

                }
            });
        }


        //function called when user updates status, spits out log to console
        private async Task UserUpdated(SocketGuildUser userBefore, SocketGuildUser userAfter)
        {
            await Task.Run(() =>
            {
                Console.WriteLine("{0}: {1} was    {2} - {3}", DateTime.Now.ToString(), userBefore.Username.ToString(), userBefore.Status.ToString(), userBefore.Game.ToString());
                Console.WriteLine("{0}: {1} is now {2} - {3}", DateTime.Now.ToString(), userAfter.Username.ToString(), userAfter.Status.ToString(), userAfter.Game.ToString());

                Database database = new Database("discord");

                if(userBefore.Status.ToString() == "Offline")
                {
                    //Generate New Session when user status changes from offline to something else

                    StringBuilder sb = new StringBuilder();

                    sb.Append(string.Format("INSERT INTO usersession  (session_start, session_end, user_id, user_ign, force_end) " +
                        "VALUES ('{0}', '{1}','{2}', '{3}','')", 
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") ,""
                        ,
                        userBefore.Id,
                        userBefore.Username
                        ));

                    Console.WriteLine(string.Format("Existing User : {0} , Executing Command UpdateUser (Online)", userBefore.Username));

                    try
                    {
                        database.ExecuteQuery(sb.ToString());
                    }
                    catch
                    {
                        Console.WriteLine(string.Format("Error: {0} , Command Existing UpdateUser (Online) Failed", userBefore.Username));
                    }

                }
                else if(userAfter.Status.ToString() == "Offline")
                {
                    //If user status goes offline, update session entry of session_end and force_end

                    StringBuilder sb = new StringBuilder();

                    sb.Append(string.Format("UPDATE usersession SET session_end = '{0}' , force_end = '0' WHERE user_id = {1} AND force_end = NULL ",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        userBefore.Id
                        ));

                    Console.WriteLine(string.Format("Existing User : {0} , Executing Command UpdateUser (Offline)", userBefore.Username));

                    try
                    {
                        database.ExecuteQuery(sb.ToString());
                    }
                    catch
                    {
                        Console.WriteLine(string.Format("Error: {0} , Command Existing UpdateUser (Offline) Failed", userBefore.Username));
                    }
                }
                else
                {

                }
            });
        }

        //collects status change and uploads to database
        //private async Task UpdateSQLServer(SocketGuildUser userBefore, SocketGuildUser userAfter)
        //{
        //    await Task.Run(() =>
        //    {
        //        string connectionString = null;
        //        SqlConnection cnn;


        //    });
        //}

        private async Task HandleCommand(SocketMessage parameterMessage)
        {
            //Don't handle the command if its a system message
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;

            //Mark where prefix ends and command begins
            int argPos = 0;

            //Determine if message has a valid prefix, and adjust argPos
            if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.HasCharPrefix('!', ref argPos))) return;

            //Create a Command Context
            var context = new CommandContext(client, message);

            //Execute the command, store the result
            var result = await commands.ExecuteAsync(context, argPos, map);

            //If the command failed, notify the user;
            if (!result.IsSuccess)
                await message.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");
        }

        
    }
}
