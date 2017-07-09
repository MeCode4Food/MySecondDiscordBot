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

        public static System.Collections.Generic.IReadOnlyCollection<SocketGuild> pGuildList;

        public async Task Install(IDependencyMap _map)
        {
            //create Command Service, inject it to Dependency map
            client = _map.Get<DiscordSocketClient>();
            commands = new CommandService();

            //_map.Add(commands);

            map = _map;

            //Adds assembly to command service
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            client.Ready += StartUserSession;

            client.MessageReceived += HandleCommand;
            client.UserJoined += AddUser;
            client.UserLeft += UserLeft;
            client.GuildMemberUpdated += UserUpdated;
        }

        //Update user session when bot comes online
        private async Task StartUserSession()
        {
            await Task.Run(() =>
            {
                Console.WriteLine("Initiate Boot Sequence");
                //Guild list in IReadOnlyCollection<SocketGuild>
                pGuildList = client.Guilds;

                //String for query (unsafe)
                StringBuilder sbsession = new StringBuilder();
                StringBuilder sbactivity = new StringBuilder();

                Console.WriteLine("Getting users from database");

                //Read from table discorduser
                var dbUserList = Database.CheckExistingUser();

                //Compare with list from guild(s?)
                
                //Add new user if not found

                //String for session query string
                sbsession.Append("INSERT INTO usersession  (session_start , session_end , user_id , user_ign , force_end , guild_id ) VALUES ");

                //String for activity query string
                sbactivity.Append("SET ANSI_WARNINGS OFF;");

                //check list of guilds the bot is 
                foreach (SocketGuild guild in pGuildList)
                {
                    Console.WriteLine(string.Format("Guild Name: {0}, Guild Id: {1}", guild.Name, guild.Id));
                    //Gets list of users in guild which are online

                    var userList = guild.Users;

                    Console.WriteLine("Updating database entry, session and activity for users");
                    foreach (SocketUser user in userList)
                    {
                        //Compare current user with database, if not found then update user database
                        if(!dbUserList.Contains(user.Id.ToString()))
                        {
                            Database.EnterUser(user);
                        }

                        if(user.Status.ToString() != "Offline")
                        {
                            //Generate new user session
                            sbsession.Append(string.Format("('{0}', '{1}','{2}','{3}','{4}','{5}'),",
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),"",user.Id, user.Username, "", guild.Id));

                            //Generate new user activity
                            sbactivity.Append(string.Format("INSERT INTO useractivity (user_ign ,session_id , status_before , status_after , game_id , timestamp) SELECT '{0}', session_id, '{1}', '{2}', '{3}', '{4}' FROM usersession WHERE user_id = '{5}' AND session_end = '';",
                                user.Username, "Console Inactive", user.Status.ToString(), user.Game.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), user.Id));


                        }
                    }
                    
                }
                //Remove comma from sbsession string and add semi colon behind
                sbsession.Remove(sbsession.Length - 1, 1);
                sbsession.Append(";");

                Console.WriteLine(string.Format("Executing Command StartUserSession"));

                //instantiate database

                Database database = new Database("discord");

                Database.QueryAnnounce(database, sbsession, "StartUserSessions");

                Database database1 = new Database("discord");

                Database.QueryAnnounce(database1, sbactivity, "StartUserActivities");
                
            });
        }

        private async Task UserLeft(SocketGuildUser user)
        {
            var database = new Database("discord");

            await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(string.Format("UPDATE discorduser SET user_isjoined = '0' WHERE user_id = {0}", user.Id ));

                Database.QueryAnnounce(database, sb, "UserLeft", user);
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
                    catch(Exception e)
                    {
                        Console.WriteLine(string.Format("Error: {0} , Command AddUser Failed", user.Username));
                        Console.WriteLine(e.ToString());
                    }

                    database.CloseConnection();
                }
                else
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(string.Format("UPDATE discorduser SET user_isjoined = '1' WHERE user_id = {0}", user.Id));

                    Database.QueryAnnounce(database, sb, "AddUser", user);
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

                StringBuilder sb = new StringBuilder();

                if (userBefore.Status.ToString() == "Offline")
                {
                    //Generate New Session when user status changes from offline to something else
                    sb.Append(string.Format("INSERT INTO usersession  (session_start, session_end, user_id, user_ign, force_end, guild_id) " +
                        "VALUES ('{0}', '{1}','{2}', '{3}','',{4});", 
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") ,""
                        ,
                        userBefore.Id,
                        userBefore.Username,
                        userBefore.Guild.Id
                        ));

                    //Update useractivity from status offline to online (or something)
                    sb.Append(string.Format("INSERT INTO useractivity (user_ign, session_id , status_before , status_after , game_id ,timestamp)" +
                      "SELECT  '{0}', session_id, '{1}', '{2}', '{3}','{4}' FROM usersession WHERE user_id = '{5}';",
                      userBefore.Username, userBefore.Status.ToString(), userAfter.Status.ToString(), userAfter.Game.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), userBefore.Id));

                    Database.QueryAnnounce(database, sb, "UpdateUser - Online", userBefore);

                }
                else if(userAfter.Status.ToString() == "Offline")
                {
                    //If user status goes offline, update session entry of session_end and force_end
                    sb.Append(string.Format("UPDATE usersession SET session_end = '{0}' , force_end = '0' WHERE user_id = {1} AND force_end = NULL ;",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        userBefore.Id
                        ));

                    //Update activity of user
                    sb.Append(string.Format("INSERT INTO useractivity (user_ign, session_id , status_before , status_after , game_id ,timestamp)" +
                      "SELECT  '{0}', session_id, '{1}', '{2}', '{3}','{4}' FROM usersession WHERE user_id = '{5}';",
                      userBefore.Username, userBefore.Status.ToString(), userAfter.Status.ToString(), userAfter.Game.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),userBefore.Id));

                    Database.QueryAnnounce(database, sb, "UpdateUser - Offline", userBefore);
                }
                else
                {
                    //Update activity of user
                    sb.Append(string.Format("INSERT INTO useractivity (user_ign, session_id , status_before , status_after , game_id ,timestamp)" +
                      "SELECT  '{0}', session_id, '{1}', '{2}', '{3}','{4}' FROM usersession WHERE user_id = '{5}';",
                      userBefore.Username, userBefore.Status.ToString(), userAfter.Status.ToString(), userAfter.Game.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),userBefore.Id));

                    Database.QueryAnnounce(database, sb, "UpdateUser - Activity", userBefore);
                }
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
