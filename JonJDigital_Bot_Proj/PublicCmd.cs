using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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

            var stm = "select * from levels where user_id=" + author.Id + " and guild_id=" + guildId;
            var cmd = new MySqlCommand(stm, con);
            
            MySqlDataReader rdr = cmd.ExecuteReader();
            
            if (rdr.Read())
            {
                var user = rdr.GetInt64(0);
                var guild = rdr.GetInt64(1);
                var exp = rdr.GetInt64(2);
                var level = rdr.GetInt64(3);

                var levelBoundary = level * 100;
                var newXp = exp + experience;
                if ( newXp >= levelBoundary)
                {
                    level++;
                    rdr.Close();
                    var sql1 = $"update levels set experience = {newXp}, level = {level} where guild_id = {guild} and user_id = {user}";
                    var cmd11 = new MySqlCommand(sql1, con);
                    cmd11.ExecuteNonQuery();
                    con.Close();
                    return $"Congrats {author.Username} you have levelled up to level {level}!!";
                }
                
                rdr.Close();
                var sql = $"update levels set experience = {newXp}, level = {level} where guild_id = {guild} and user_id = {user}";
                var cmd1 = new MySqlCommand(sql, con);
                cmd1.ExecuteNonQuery();
                con.Close();
                // Console.WriteLine("row inserted");
                // Console.WriteLine($"User: {user}, Guild: {guild}, Experience: {exp}, Level: {level}");
                // Console.WriteLine("{0}", rdr.GetInt64(0));
            }
            else
            {
                rdr.Close();
                var sql = $"INSERT INTO levels(user_id, guild_id, experience, level) VALUES({author.Id}, {guildId}, {experience}, 0)";
                var cmd1 = new MySqlCommand(sql, con);
                cmd1.ExecuteNonQuery();
                con.Close();
                // Console.WriteLine("row inserted");
                // return "Gained: " + experience + " xp";
            }

            return "";
            // Console.WriteLine(result);
        }
    }
    
}