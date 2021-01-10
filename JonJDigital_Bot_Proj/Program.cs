using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace JonJDigital_Bot_Proj
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {

            string prefix = Environment.GetEnvironmentVariable("DISCORD_PREFIX");
            string token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
            
            var Discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot
            });

            Discord.MessageCreated += async (s, e) =>
            {
                PublicCmd publicCmd = new PublicCmd();

                //ignore bots own messages
                if(e.Message.Author.IsBot) return;
                
                //declare jons account IDs
                ulong jonjdigital = 229244232064434177;
                ulong jonjdigital_test = 691346983494877216;
                
                string msg = e.Message.Content;
                var msgAuthor = e.Message.Author;
                ulong author = e.Message.Author.Id;

                var response = publicCmd.levelUp(e.Message);
                if (response != "")
                {
                    e.Message.RespondAsync(response);
                }

                //publicly available commands
                if (msg.ToLower().StartsWith(prefix + "profile"))
                {
                    var mentions = e.Message.MentionedUsers;
                    
                    if (mentions.Count == 0)
                    {
                        e.Message.RespondAsync(embed: publicCmd.profile(e.Message));
                    }
                    else
                    {
                        var user = await e.Message.Channel.Guild.GetMemberAsync(mentions.First().Id);
                        var embed = publicCmd.otherprofile(e.Message, user);
                        e.Message.RespondAsync(embed: embed);
                    }
                }

                if(msg.ToLower().StartsWith(prefix + "twitch"))
                {
                    e.Message.RespondAsync(embed: publicCmd.twitchProfile(e.Message));
                }

                if (msg.ToLower().StartsWith(prefix + "video"))
                {
                    e.Message.RespondAsync(embed: publicCmd.youtubeQuery(e.Message));
                }

                
                //admin commands start
                if (msg.ToLower().StartsWith(prefix + "reset"))
                {
                    if (publicCmd.checkAdmin(e.Message))
                    {
                        var mentions = e.Message.MentionedUsers;

                        if (mentions.Count == 0)
                        {
                            e.Message.RespondAsync($"<@{msgAuthor.Id}>, please mention a user to reset a profile");
                        }
                        else
                        {
                            var user = await e.Message.Channel.Guild.GetMemberAsync(mentions.First().Id);
                            e.Message.RespondAsync(embed: publicCmd.resetProfile(e.Message, user));

                        }
                    }
                    else
                    {
                        e.Message.RespondAsync($"<@{author}>, you do not have access to this command");

                    }
                }
                
                if (msg.ToLower() == prefix + "ping")
                {
                    // var author = e.Author.Id;
                    if (author != jonjdigital && author != jonjdigital_test)
                    {                        
                        e.Message.RespondAsync("Sorry <@" + e.Message.Author.Id + ">, but this is Developer only command!");
                    }else
                    {                        
                        e.Message.RespondAsync(publicCmd.ping());
                    }
                }

            };
            
            await Discord.ConnectAsync();
            await Task.Delay(-1);

        }
    }
}