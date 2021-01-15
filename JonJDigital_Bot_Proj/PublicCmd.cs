﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using DSharpPlus;
using DSharpPlus.Entities;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Web;
using Emzi0767.Utilities;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.Utilities.Encoders;
using RestSharp;
using Method = Google.Protobuf.WellKnownTypes.Method;

namespace JonJDigital_Bot_Proj
{
    public class PublicCmd
    {
        private static string TwitchID = Environment.GetEnvironmentVariable("DISCORD_TWITCH_CLIENT");
        private static string TwitchSecret = Environment.GetEnvironmentVariable("DISCORD_TWITCH_SECRET");
        private static string YoutubeAPI = Environment.GetEnvironmentVariable("DISCORD_YOUTUBE_API");
        private static string redirect = WebUtility.UrlEncode("https://localhost");
        private string authUri = $"https://id.twitch.tv/oauth2/token?client_id={TwitchID}&client_secret={TwitchSecret}&grant_type=client_credentials";
        
        private static string conStr = Environment.GetEnvironmentVariable("DISCORD_MYSQL");
        private string TwitchApiUsers = "https://api.twitch.tv/helix/users";
        private string TwitchChannelSearch = "https://api.twitch.tv/helix/search/channels";
        private string TwitchGameSearch = "https://api.twitch.tv/helix/games";
        private string YoutubeSearch = "https://www.googleapis.com/youtube/v3/search?part=snippet&maxResults=3&q="; //add keyword and key onto end
        private MySqlConnection con = new MySqlConnection(conStr);
        
        public bool checkAdmin(DiscordMessage msg)
        {
            var guild = msg.Channel.Guild;
            DiscordMember member = guild.GetMemberAsync(msg.Author.Id).Result;
            
            if(member.PermissionsIn(msg.Channel).HasPermission(Permissions.Administrator))return true; //overall admin
            if(member.PermissionsIn(msg.Channel).HasPermission(Permissions.KickMembers))return true; //user admin
            if(member.PermissionsIn(msg.Channel).HasPermission(Permissions.BanMembers))return true; //user admin
            if(member.PermissionsIn(msg.Channel).HasPermission(Permissions.ManageChannels))return true; //server admin
            if(member.PermissionsIn(msg.Channel).HasPermission(Permissions.ManageEmojis))return true; //server admin
            if(member.PermissionsIn(msg.Channel).HasPermission(Permissions.ManageGuild))return true; //server admin
            if(member.PermissionsIn(msg.Channel).HasPermission(Permissions.ManageMessages))return true; //server admin
            if(member.PermissionsIn(msg.Channel).HasPermission(Permissions.ManageNicknames))return true; //user admin
            if(member.PermissionsIn(msg.Channel).HasPermission(Permissions.ManageWebhooks))return true; //server admin 
            if(member.PermissionsIn(msg.Channel).HasPermission(Permissions.ManageRoles))return true; //server admin
            return false;
        }
        public bool checkUserAdmin(DiscordMessage msg)
        {
            var guild = msg.Channel.Guild;
            DiscordMember member = guild.GetMemberAsync(msg.Author.Id).Result;
            
            if(member.PermissionsIn(msg.Channel).HasPermission(Permissions.Administrator))return true; //overall admin
            if(member.PermissionsIn(msg.Channel).HasPermission(Permissions.KickMembers))return true; //user admin
            if(member.PermissionsIn(msg.Channel).HasPermission(Permissions.BanMembers))return true; //user admin
            if(member.PermissionsIn(msg.Channel).HasPermission(Permissions.ManageNicknames))return true; //user admin
            return false;
        }
        public string getRoles(ulong user, DiscordMessage msg)
        {
            var guild = msg.Channel.Guild;
            DiscordMember member = guild.GetMemberAsync(user).Result;
            var roles = member.Roles.ToArray();
            // var roles2 = roles.ToArray();
            var memberRoles = new List<string>();
            foreach (var role in roles)
            {
                memberRoles.Add(new string(role.Name));
            }

            var rolesRet = memberRoles.ToArray();
            string roleList = String.Join(", ", rolesRet);
            // Console.WriteLine;
            return roleList;
        }
        public string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
        public PublicCmd()
        {

        }
        public string ping()
        {
            con.Open();
            string version = con.ServerVersion;
            con.Close();
            // Console.WriteLine(windir);
            return "MySQL version : "+version;
        }
        public string levelUp(DiscordMessage msg)
        {
            var guildId = msg.Channel.Guild.Id;
            var author= msg.Author;
            double exp1 = msg.Content.Length / 2;
            var experience = Math.Ceiling(exp1);
            con.Open();

            var stm = $"select * from levels where user_id={author.Id} and guild_id={guildId}";
            var cmd = new MySqlCommand(stm, con);
            
            MySqlDataReader rdr = cmd.ExecuteReader();
            
            if (rdr.Read())
            {
                var user = rdr.GetInt64(0);
                var guild = rdr.GetInt64(1);
                var exp = rdr.GetInt64(2);
                var currLevel = rdr.GetInt64(3);

                // var levelBoundary = (((currLevel * 20) * currLevel * 0.8) + currLevel * 100) - 16;
                var level = currLevel;
                var newXp = exp + experience;
                while (newXp >= ((((level * 20) * level * 0.8) + level * 100) - 16))
                {
                    level++;
                }
                rdr.Close();
                var sql1 = $"update levels set experience = {newXp}, level = {level} where guild_id = {guild} and user_id = {user}";
                var cmd11 = new MySqlCommand(sql1, con);
                cmd11.ExecuteNonQuery();
                con.Close();
                if (level != currLevel)
                {
                    return $"Congrats {author.Username} you have levelled up to level {level}!!";
                } 
            }
            else
            {
                var level = 1;
                while (experience >= ((((level * 20) * level * 0.8) + level * 100) - 16))
                {
                    level++;
                }
                rdr.Close();
                var sql = $"INSERT INTO levels(user_id, guild_id, experience, level) VALUES({author.Id}, {guildId}, {experience}, {level})";
                var cmd1 = new MySqlCommand(sql, con);
                cmd1.ExecuteNonQuery();
                con.Close();
                return $"Congrats {author.Username} you have levelled up to level {level}!!";

            }
            return "";
        }
        public DiscordEmbed profile(DiscordMessage msg)
        {
            var author = msg.Author;
            var guild = msg.Channel.Guild;
            var roles = getRoles(author.Id, msg);
            DiscordMember member = guild.GetMemberAsync(author.Id).Result;
            con.Open();

            var stm = $"select * from levels where user_id={author.Id} and guild_id={guild.Id}";
            var cmd = new MySqlCommand(stm, con);
            
            MySqlDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                var exp = rdr.GetInt64(2).ToString();
                var currLevel = rdr.GetInt64(3).ToString();
                rdr.Close();
                var embed = new DiscordEmbedBuilder
                {
                    Color = member.Color,
                    Title = $"Profile For: {msg.Author.Username}#{msg.Author.Discriminator}",
                    Timestamp = DateTime.UtcNow,
                };
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                {
                  Url = msg.Author.AvatarUrl
                };
                embed.AddField("Server: ", guild.Name, false);
                embed.AddField("Level: ", currLevel, true);
                embed.AddField("Experience: ", exp, true);
                embed.AddField("Roles: ", roles, true);
                embed.WithFooter(msg.Author.Username, msg.Author.AvatarUrl);

                return embed.Build();
            }

            return null;
        }
        public DiscordEmbed otherprofile(DiscordMessage msg, DiscordUser mentioned)
        {
            var author = msg.Author;
            var guild = msg.Channel.Guild;
            var roles = getRoles(mentioned.Id, msg);
            DiscordMember member = guild.GetMemberAsync(mentioned.Id).Result;
            con.Open();

            var stm = $"select * from levels where user_id={mentioned.Id} and guild_id={guild.Id}";
            var cmd = new MySqlCommand(stm, con);
            
            MySqlDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                var exp = rdr.GetInt64(2).ToString();
                var level = rdr.GetInt64(3).ToString();
                rdr.Close();
                var reply = new DiscordEmbedBuilder()
                {
                    Color = member.Color,
                    Title = $"Profile For: {mentioned.Username}#{mentioned.Discriminator}",
                    Timestamp = DateTime.UtcNow,
                };
                reply.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                {
                    Url = mentioned.AvatarUrl
                };
                reply.AddField("Server: ", guild.Name, false);
                reply.AddField("Level: ", level, true);
                reply.AddField("Experience: ", exp, true);
                reply.AddField("Roles: ", roles, true);
                reply.WithFooter(mentioned.Username, mentioned.AvatarUrl);

                return reply.Build();
            }
            else
            {
                rdr.Close();
                var reply = new DiscordEmbedBuilder()
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"Profile For: {mentioned.Username}#{mentioned.Discriminator}",
                    Timestamp = DateTime.UtcNow,
                };
                reply.Description = "This User has either not spoken, or is a Bot";
                return reply.Build();
            }
        }
        public DiscordEmbed twitchProfile(DiscordMessage msg)
        {
            var content = msg.Content.Split(" ");
            return TwitchRequest(content[1]);
        }
        private DiscordEmbed TwitchRequest(String user)
        {
            
            string AccessToken = getTwitchAccess();
            
            //get user channel info
            string url = TwitchApiUsers + $"?login={user}";
            var restClient = new RestClient(url);
            restClient.AddDefaultHeader("Authorization", "Bearer "+AccessToken);
            restClient.AddDefaultHeader("Client-ID", TwitchID);
            restClient.Timeout = -1;
            var request = new RestRequest(RestSharp.Method.GET);
            string response = restClient.Execute(request).Content;
            var userArray = (JObject) JsonConvert.DeserializeObject(response);
            var data = userArray["data"][0];

            string displayName = (string) data["display_name"];
            string status = (string) data["broadcaster_type"];
            string profileImage = (string) data["profile_image_url"];

            //get the channel info such as is user currently streaming
            string channelUrl = TwitchChannelSearch + $"?query={user}";
            var restClient1 = new RestClient(channelUrl);
            restClient1.AddDefaultHeader("Authorization", "Bearer "+AccessToken);
            restClient1.AddDefaultHeader("Client-ID", TwitchID);
            restClient1.Timeout = -1;
            var request1 = new RestRequest(RestSharp.Method.GET);
            string response1 = restClient1.Execute(request1).Content;
            var channelArray = (JObject) JsonConvert.DeserializeObject(response1);
            var channel = channelArray["data"][0];
            string live = (string) channel["is_live"];
            
            var embed = new DiscordEmbedBuilder()
            {
                Color = new DiscordColor("#7821a3"),
                Title = displayName,
                Timestamp = DateTime.UtcNow,
            };
            if (status != "")
            {
                embed.AddField("Channel Type: ", FirstLetterToUpper(status), true);
            }
            else
            {
                embed.AddField("Channel Type: ", "Normal", true);
            }
            
            embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
            {
                Url = profileImage
            };
            embed.AddField("Views: ", (string) data["view_count"], true);
            embed.Url = $"https://twitch.tv/{user}";
            
            if (live == "True")
            {
                embed.AddField("Status: ", ":red_circle:", true);
                string gameId = (string) channel["game_id"];
                
                //get the game info if user is LIVE
                string gameUrl = TwitchGameSearch + $"?id={gameId}";
                var restClient2 = new RestClient(gameUrl);
                restClient2.AddDefaultHeader("Authorization", "Bearer "+AccessToken);
                restClient2.AddDefaultHeader("Client-ID", TwitchID);
                restClient2.Timeout = -1;
                var request2 = new RestRequest(RestSharp.Method.GET);
                string response2 = restClient2.Execute(request2).Content;
                var gameArray = (JObject) JsonConvert.DeserializeObject(response2);
                var game = gameArray["data"][0];
                
                embed.Description = $"{displayName} is streaming {(string) game["name"]}";
            }
            else
            {
                embed.AddField("Status: ", ":zzz:", true);
                embed.Description = $"{displayName} is currently not streaming.";
            }
            return embed.Build();
        }
        private string getTwitchAccess()
        {
            var restClient = new RestClient(authUri);
            restClient.Timeout = -1;
            var request = new RestRequest(RestSharp.Method.POST);
            IRestResponse response = restClient.Execute(request);
            var response2 = response.Content;
            var accessArray = (JObject) JsonConvert.DeserializeObject(response2);
            // var token = accessArray;

            return (string) accessArray["access_token"];
        }
        public DiscordEmbed youtubeQuery(DiscordMessage msg)
        {

            string[] content = msg.Content.Split(" ");
            var args = content.Skip(1).ToArray();
            var args1 = String.Join("+",args);
            // Console.WriteLine(args1);

            string url = YoutubeSearch + args1 + "&key=" + YoutubeAPI;
            
            var restClient = new RestClient(url);
            restClient.Timeout = -1;
            var request = new RestRequest(RestSharp.Method.GET);
            request.AddHeader("Accept", "application/http");
            IRestResponse response = restClient.Execute(request);
            var response2 = response.Content;
            var videoArray = (JObject) JsonConvert.DeserializeObject(response2);
            
            // Console.WriteLine(videoArray);

            foreach (var result in videoArray["items"])
            {
                var itemType = result["id"]["kind"].ToString();
                if (itemType == "youtube#video")
                {
                    var videoId = result["id"];
                    var videoSnippet = result["snippet"];
                    // Console.WriteLine(videoSnippet);
                    var embed = new DiscordEmbedBuilder()
                    {
                        Color = new DiscordColor("#ed0251"),
                        Title = (string) videoSnippet["title"],
                        Timestamp = DateTime.UtcNow,
                        Url = $"https://youtube.com/watch?v={(string) videoId["videoId"]}",
                        ImageUrl = (string) videoSnippet["thumbnails"]["medium"]["url"],
                    };

                    // var date = DateTime.Parse((string) videoSnippet["publishTime"]).ToLongDateString();
                    var date = Strings.Replace((string) videoSnippet["publishTime"],"T"," ");
                    embed.AddField("Uploaded: ", date);
                    embed.WithFooter((string) videoSnippet["channelTitle"]);


                    return embed.Build();
                }
            }

            var embed2 = new DiscordEmbedBuilder()
            {
                Color = new DiscordColor("#ed0251"),
                Title = "No Video Found",
                Timestamp = DateTime.UtcNow,
            };

            return embed2.Build();
            
            
        }
        public DiscordEmbed resetProfile(DiscordMessage msg, DiscordUser mentioned)
        {
            var guild = msg.Channel.Guild;
            con.Open();

            var stm = $"update levels set experience = 0, level = 0 where user_id={mentioned.Id} and guild_id={guild.Id}";
            var cmd = new MySqlCommand(stm, con);
            
            cmd.ExecuteReader();
            
            var reply1 = new DiscordEmbedBuilder()
            {
                Color = new DiscordColor("#FF0000"),
                Title = $"Profile has been reset for {mentioned.Username}#{mentioned.Discriminator}",
                Timestamp = DateTime.UtcNow,
            };
            reply1.AddField("Server: ", guild.Name, true);
            reply1.AddField("Level: ", "0", true);
            reply1.AddField("Experience: ", "0", true);
            reply1.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
            {
                Url = mentioned.AvatarUrl
            };

            return reply1.Build();
        }

        public DiscordEmbed addRole(DiscordMessage msg, DiscordUser user, DiscordRole role)
        {
            DiscordGuild guild = msg.Channel.Guild;
            DiscordMember member = guild.GetMemberAsync(user.Id).Result;
            DiscordMember admin = guild.GetMemberAsync(msg.Author.Id).Result;
            member.GrantRoleAsync(role);

            var reply = new DiscordEmbedBuilder()
            {
                Color = new DiscordColor("#3fc5e0"),
                Title = $"The role {role.Name} has been added to {member.DisplayName}.",
                Timestamp = DateTime.UtcNow,
            };
            reply.AddField("Server: ", guild.Name, true);
            reply.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
            {
                Url = user.AvatarUrl
            };
            reply.WithFooter(admin.Username+"#"+admin.Discriminator, admin.AvatarUrl);

            return reply.Build();

        }
        public DiscordEmbed removeRole(DiscordMessage msg, DiscordUser user, DiscordRole role)
        {
            DiscordGuild guild = msg.Channel.Guild;
            DiscordMember member = guild.GetMemberAsync(user.Id).Result;
            DiscordMember admin = guild.GetMemberAsync(msg.Author.Id).Result;
            member.RevokeRoleAsync(role);

            var reply = new DiscordEmbedBuilder()
            {
                Color = new DiscordColor("#3fc5e0"),
                Title = $"The role {role.Name} has been removed from {member.DisplayName}.",
                Timestamp = DateTime.UtcNow,
            };
            reply.AddField("Server: ", guild.Name, true);
            reply.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
            {
                Url = user.AvatarUrl
            };
            reply.WithFooter(admin.Username+"#"+admin.Discriminator, admin.AvatarUrl);

            return reply.Build();

        }
    }
    
}