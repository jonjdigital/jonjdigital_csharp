using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using DSharpPlus;
using DSharpPlus.Entities;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace JonJDigital_Bot_Proj
{
    public class PublicCmd
    {
        public PublicCmd()
        {
        }

        public string helloWorld(DiscordMessage msg)
        {
            return "H";
        }
        public string ping()
        {
            // DotNetEnv.Env.Load("../../../.env");
            string conStr = DotNetEnv.Env.GetString("MYSQL_CON");

            var con = new MySqlConnection(conStr);
            con.Open();
            string version = con.ServerVersion;
            con.Close();
            return "MySQL version : "+version;
        }

        public string levelUp(DiscordMessage msg)
        {
            var guildId = msg.Channel.Guild.Id;
            var author= msg.Author;
            double exp1 = msg.Content.Length / 2;
            var experience = Math.Ceiling(exp1);
            
            string conStr = DotNetEnv.Env.GetString("MYSQL_CON");

            var con = new MySqlConnection(conStr);
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
            
            string conStr = DotNetEnv.Env.GetString("MYSQL_CON");

            var con = new MySqlConnection(conStr);
            con.Open();

            var stm = $"select * from levels where user_id={author.Id} and guild_id={guild.Id}";
            var cmd = new MySqlCommand(stm, con);
            
            MySqlDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                // var user = rdr.GetInt64(0);
                // var server = rdr.GetInt64(1);
                var exp = rdr.GetInt64(2).ToString();
                var currLevel = rdr.GetInt64(3).ToString();
                rdr.Close();
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
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
                embed.WithFooter(msg.Author.Username, msg.Author.AvatarUrl);
                
                return embed.Build();
                // msg.Respond(embed: embed.Build());
                // return embed;
            }

            return null;
            // return "";
        }
    }
    
}