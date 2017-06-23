using System;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;

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
            client.GuildMemberUpdated += UserUpdated;
        }

        
        //function called when user updates status, spits out log to console
        private async Task UserUpdated(SocketGuildUser userBefore, SocketGuildUser userAfter)
        {
            await Task.Run(() =>
            {
                Console.WriteLine("{0}: {1} was    {2} - {3}", DateTime.Now.ToString(), userBefore.Username.ToString(), userBefore.Status.ToString(), userBefore.Game.ToString());
                Console.WriteLine("{0}: {1} is now {2} - {3}", DateTime.Now.ToString(), userAfter.Username.ToString(), userAfter.Status.ToString(), userAfter.Game.ToString());
            });
        }

        //collects status change and uploads to database
        private async Task UpdateSQLServer(SocketGuildUser userBefore, SocketGuildUser userAfter)
        {
            await Task.Run(() =>
            {
                string connectionString = null;
                SqlConnection cnn;


            });
        }

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
