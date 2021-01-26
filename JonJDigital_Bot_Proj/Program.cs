using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using MySql.Data.MySqlClient;

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

            if (prefix != "jb")
            {
                MySqlConnection con = new MySqlConnection(Environment.GetEnvironmentVariable("DISCORD_MYSQL"));
                con.Open();
                string version = con.ServerVersion;
                Console.WriteLine(version);
                con.Close();
                var smtp = new SmtpClient("smtp.office365.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("bots@jonjdigital.com","J0nJD!g1t@L"),
                    EnableSsl = true,
                };

                var date = DateTime.Now;
                MailAddress sender = new MailAddress("bots@jonjdigital.com", "Bots");
                MailMessage message = new MailMessage()
                {
                    From = sender,
                    Subject = "Discord Bot AutoStart Email",
                    Body =
                        $"<h1>The discord bot has started up.</h1><hr>This is a courtesy ping to the DB to ensure connection.<br><br><hr>Mysql Server Version: {version}<hr>Date: {date.Day}/{date.Month}/{date.Year}, Time: {date.Hour}:{date.Minute}",
                    IsBodyHtml = true,
                };
                
                message.To.Add("jon.james@jonjdigital.com");
                smtp.Send(message);


            }
            
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
                DiscordUser msgAuthor = e.Message.Author;
                ulong author = e.Message.Author.Id;

                var response = publicCmd.levelUp(e.Message);
                if (response != "")
                {
                    await e.Message.RespondAsync(response);
                }

                //publicly available commands
                if (msg.ToLower().StartsWith(prefix + "profile"))
                {
                    var mentions = e.Message.MentionedUsers;
                    
                    if (mentions.Count == 0)
                    {
                        await e.Message.RespondAsync(embed: publicCmd.profile(e.Message));
                    }
                    else
                    {
                        var user = await e.Message.Channel.Guild.GetMemberAsync(mentions.First().Id);
                        var embed = publicCmd.otherprofile(e.Message, user);
                        await e.Message.RespondAsync(embed: embed);
                    }
                }

                if(msg.ToLower().StartsWith(prefix + "twitch"))
                {
                    await e.Message.RespondAsync(embed: publicCmd.twitchProfile(e.Message));
                }

                if (msg.ToLower().StartsWith(prefix + "video"))
                {
                    await e.Message.RespondAsync(embed: publicCmd.youtubeQuery(e.Message));
                }

                if (msg.ToLower() == prefix + "enable")
                {
                    await e.Message.RespondAsync(embed: publicCmd.userGuildUnmute(e.Message.Channel, e.Message));
                }
                
                if (msg.ToLower() == prefix + "disable")
                {
                    await e.Message.RespondAsync(embed: publicCmd.userGuildMute(e.Message.Channel, e.Message));
                }
                
                //admin commands start
                if (msg.ToLower().StartsWith(prefix + "reset"))
                {
                    await e.Message.DeleteAsync();
                    
                    if (publicCmd.checkUserAdmin(e.Message))
                    {
                        var mentions = e.Message.MentionedUsers;

                        if (mentions.Count == 0)
                        {
                            await e.Message.RespondAsync($"<@{msgAuthor.Id}>, please mention a user to reset a profile");
                        }
                        else
                        {
                            var user = await e.Message.Channel.Guild.GetMemberAsync(mentions.First().Id);
                            await e.Message.RespondAsync(embed: publicCmd.resetProfile(e.Message, user));

                        }
                    }
                    else
                    {
                        await e.Message.RespondAsync($"<@{author}>, you do not have access to this command. You need to be able to Manage Users to use this command.");

                    }
                }
                
                if (msg.ToLower() == prefix + "ping")
                {
                    await e.Message.DeleteAsync();
                    
                    // var author = e.Author.Id;
                    if (author != jonjdigital && author != jonjdigital_test)
                    {                        
                        await e.Message.RespondAsync("Sorry <@" + e.Message.Author.Id + ">, but this is Developer only command!");
                    }else
                    {                        
                        await e.Message.RespondAsync(publicCmd.ping());
                    }
                }

                if (msg.ToLower().StartsWith(prefix + "ar"))
                {
                    await e.Message.DeleteAsync();
                    
                    if (publicCmd.checkServerAdmin(e.Message))
                    {
                        DiscordUser user = e.Message.MentionedUsers.First();
                        DiscordRole role = e.Message.MentionedRoles.First();
                        await e.Message.RespondAsync(embed: publicCmd.addRole(e.Message, user, role));
                    }else
                        await e.Message.RespondAsync($"<@{author}>, you do not have access to this command. You need to be able to Manage Users to use this command.");

                }

                if (msg.ToLower().StartsWith(prefix + "rr"))
                {
                    await e.Message.DeleteAsync();
                    
                    if (publicCmd.checkServerAdmin(e.Message))
                    {
                        DiscordUser user = e.Message.MentionedUsers.First();
                        DiscordRole role = e.Message.MentionedRoles.First();
                        await e.Message.RespondAsync(embed: publicCmd.removeRole(e.Message, user, role));
                    }else
                        await e.Message.RespondAsync($"<@{author}>, you do not have access to this command. You need to be able to Manage Users to use this command.");

                }

                if ((msg.ToLower().StartsWith(prefix + "channelmute"))||(msg.ToLower().StartsWith(prefix + "mutechannel"))||(msg.ToLower().StartsWith(prefix + "cm")))
                {
                    DiscordMessage message = e.Message;
                    await message.DeleteAsync();
                    if (publicCmd.checkServerAdmin(e.Message))
                    {
                        DiscordChannel channel = message.MentionedChannels.First();

                        await message.RespondAsync(embed: publicCmd.muteChannel(channel, message));
                    }
                    else
                    {
                        await message.RespondAsync($"Apologies <@{message.Author.Id}>, but only an admin can use this command");
                    }
                }
                
                if ((msg.ToLower().StartsWith(prefix + "channelunmute"))||(msg.ToLower().StartsWith(prefix + "unmutechannel"))||(msg.ToLower().StartsWith(prefix + "cu")))
                {
                    DiscordMessage message = e.Message;
                    await message.DeleteAsync();
                    if (publicCmd.checkServerAdmin(e.Message))
                    {
                        DiscordChannel channel = message.MentionedChannels.First();

                        await message.RespondAsync(embed: publicCmd.unmuteChannel(channel, message));
                    }
                    else
                    {
                        await message.RespondAsync($"Apologies <@{message.Author.Id}>, but only an admin can use this command");
                    }
                }

                if (msg.ToLower() == prefix + "muted")
                {
                    DiscordMessage message = e.Message;
                    if (publicCmd.checkServerAdmin(message))
                    {
                        await message.RespondAsync(embed: publicCmd.listBlacklistedChannels(message));
                    }
                }
                
                //Moderation commands start

                if (msg.ToLower().StartsWith(prefix + "kick"))
                {
                    if (publicCmd.checkServerAdmin(e.Message))
                    {
                        int userCount = e.Message.MentionedUsers.Count();
                        DiscordUser mentioned = e.Message.MentionedUsers.First();

                        if (userCount != 1)
                        {
                            await e.Message.RespondAsync($"<@{author}>, please mention one user!");
                        }
                        else
                        {
                            DiscordGuild guild = e.Message.Channel.Guild;
                            DiscordMember member = guild.GetMemberAsync(mentioned.Id).Result;

                            await e.Message.RespondAsync(embed: publicCmd.kickUser(member, guild, e.Message));

                        }
                    }
                    else
                    {
                        await e.Message.RespondAsync($"Apologies <@{author}>, but only an Admin can use this command");
                    }
                }
            };
            
            await Discord.ConnectAsync();
            await Task.Delay(-1);

        }
    }
}