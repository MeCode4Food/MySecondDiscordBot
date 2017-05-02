using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MySecondDiscordBot
{
    public class DiscordModules : ModuleBase
    {
        Random random = new Random();
        [Command("Hello")]
        [Alias("hi")]
        [Summary("says 'world!'")]
        public async Task Hello()
        {
            await ReplyAsync("world!");
        }


        [Command("Say")]
        [Alias("echo")]
        [Summary("Echoes the provided input")]
        public async Task Say([Remainder] string input)
        {
            await ReplyAsync(input);
        }

        [Command("test")]
        [Alias("testing")]
        [Summary("Test Command")]
        public async Task Test(params string[] str)
        {
            await ReplyAsync(string.Join(" ", str));
        }

        [Command("info")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"{Format.Bold("Info")}\n" +
                $"- Author: {application.Owner.Username} (ID {application.Owner.Id}\n" +
                $"- Library: Discord.Net ({DiscordConfig.Version}\n" +
                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                $"- Uptime: {GetUptime()}\n\n"
                );
        }

        [Command("whoami")]
        [Summary("Summarizes who is the user")]
        public async Task WhoAmI()
        {
            //Console.WriteLine(string.Format("{0}", Context.User));

            if (string.Format("{0}", Context.User) == "cheecken0#7481")
            {
                await ReplyAsync(string.Format("Welcome bek glorious master {0}", Context.User.Mention));
            }
            else
            {
                await ReplyAsync(string.Format("{0} Yer a piss of shiet.", Context.User.Mention));
            }
        }

        [Command("announce")]
        [Summary("makes the bot say something in a given channel. !announce <channel> <message>")]
        public async Task Announce(params string[] str)
        {
            Discord.WebSocket.SocketTextChannel channel = null;
            bool isFirstArgChannel = false;

            if (str.Length == 0)
            {
                await ReplyAsync("The format for this command is !announce <channel> <message>");
            }
            else
            {
                //Argument of GetChannleAsync is obtained through the ChannelID on discord, through enabling developer mode
                switch (string.Format(str[0]))
                {

                    case "general":
                        channel = await Context.Guild.GetChannelAsync(117894147792175108) as SocketTextChannel;
                        isFirstArgChannel = true;
                        break;
                    case "thelab":
                        channel = await Context.Guild.GetChannelAsync(304502202490290176) as SocketTextChannel;
                        isFirstArgChannel = true;
                        break;
                    default:
                        await Context.Channel.SendMessageAsync("Please include a proper text channel name.");
                        break;
                }
            }

            
            
            if(isFirstArgChannel)
            {
                await channel.SendMessageAsync(ConstructMessage(str));
            }

        }

        [Command("starcraft")]
        [Summary("Spits out hot garbage about starcraft 2")]
        public async Task Starcraft()
        {
            await ReplyAsync("LEL fite mi 1v1 in Starcraft 2\nChick");
        }

        [Command("help")]
        [Summary("Spits out available commands like a boss.")]
        public async Task Help()
        {
            //await Context.Channel.SendMessageAsync("test").ConfigureAwait(false);
            await ReplyAsync("Discord Bot ChickBot v0.3 by Chick\n\n");
            await ReplyAsync("Here are the current list of commands:\n");
            await ReplyAsync("- !starcraft : Spits out hot garbage about 1v1 in Starcraft.\n");
            await ReplyAsync("- !hello : Converse with the bot you lonely ass. Bot replies 'world!'. \n");
            await ReplyAsync("- !whoami : Bot identifies your being and replies in kind.\n");
            await ReplyAsync("- !announce 'message' : ChickBot spits out the message, telling people you said it. \n");
            await ReplyAsync("- !pun : Forces bot to spit out bad puns from a certain pun.txt");

        }

        
        [Command("leave")]
        [Summary("Forces bot to leave his home. Bot is now sad")]
        public async Task Leave()
        {
            if (string.Format("{0}", Context.User) == "cheecken0#7481")
            {
                await ReplyAsync("Yes Master, at your command.");
                await Context.Guild.LeaveAsync();
            }
            else
                await ReplyAsync("YOU CAN'T MAKE ME YOU FOO");

        }

        [Command("pun")]
        [Alias("joke")]
        [Summary("Forces bot to shit jokes from a certain pun.txt hidden somewhere in its files")]
        public async Task Pun()
        {
            string[] jokes = File.ReadAllLines(@"C:\Users\Inconspicuous\Source\Repos\MySecondDiscordBot\MySecondDiscordBot\pun.txt",Encoding.ASCII);

            int randomNumber = random.Next(0, jokes.Length);

            StringBuilder buildLOL = new StringBuilder();

            //append to stringbuilder buildLOL
            bool isN = false;

            foreach (char c in jokes[randomNumber])
            {
                
                if (c == '\\')
                {
                    isN = true;
                }
                else if((c == 'n') && isN)
                {
                    buildLOL.AppendLine();
                    isN = false;
                }
                else
                    buildLOL.Append(c);
            }

            await Context.Channel.SendMessageAsync(buildLOL.ToString());
            //await ReplyAsync("done pls chek dengz");
        }

        private static string GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");

        //DoAnnouncement method which calls the ConstructMessage method to create a message based on arguments on !announce
        //private async Task DoAnnouncement(CommandEventArgs e)
        //{
        //    Console.WriteLine("{0}", e.Args[0]);

        //    var channel = e.Server.FindChannels(e.Args[0], ChannelType.Text).FirstOrDefault();

        //    var message = ConstructMessage(e, channel != null);

        //    if (channel != null)
        //    {
        //        await channel.SendMessage(message);
        //    }
        //    else
        //    {
        //        await e.Channel.SendMessage(message);
        //    }
        //}

        //Message constructor when called by DoAnnouncement
        private string ConstructMessage(string[] str)
        {
            string message = "";

            int startIndex = 1;

            for (int i = startIndex; i < str.Length; i++)
            {
                message += str[i].ToString() + " ";
            }
            
            return message;
        }

    }
}
