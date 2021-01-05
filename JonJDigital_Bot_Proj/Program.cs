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

            string prefix = "j";
            
            DotNetEnv.Env.Load("../../../.env");
            string token = DotNetEnv.Env.GetString("TOKEN");
            
            var Discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot
            });

            Discord.MessageCreated += async (s, e) =>
            {
                PublicCmd test1 = new PublicCmd();

                //ignore bots own messages
                if(e.Message.Author.IsBot) return;
                
                //declare jons account IDs
                ulong jonjdigital = 229244232064434177;
                ulong jonjdigital_test = 691346983494877216;
                
                string msg = e.Message.Content;
                var msgAuthor = e.Message.Author;
                ulong author = e.Message.Author.Id;

                var response = test1.levelUp(e.Message);
                if (response != "")
                {
                    e.Message.RespondAsync(response);
                }

                if (msg.ToLower().StartsWith(prefix + "profile"))
                {
                    var mentions = e.Message.MentionedUsers;
                    
                    if (mentions.Count == 0)
                    {
                        e.Message.RespondAsync(embed: test1.profile(e.Message));
                    }
                    else
                    {
                        var user = await e.Message.Channel.Guild.GetMemberAsync(mentions.First().Id);
                        var embed = test1.otherprofile(e.Message, user);
                        e.Message.RespondAsync(embed: embed);
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
                        e.Message.RespondAsync(test1.ping());
                    }
                }

            };
            
            await Discord.ConnectAsync();
            await Task.Delay(-1);

        }
    }
}