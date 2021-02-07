using System;
using DSharpPlus.Entities;
using MySql.Data.MySqlClient;

namespace JonJDigital_Bot_Proj
{
    public class GuildLeaderboard
    {
        private static string conStr = Environment.GetEnvironmentVariable("DISCORD_MYSQL");
        private MySqlConnection con = new MySqlConnection(conStr);

        public DiscordEmbed getLeaderBoard(DiscordGuild guild)
        {

            ulong guild_id = guild.Id;
            
            con.Open();
            string query = $"Select * from levels where guild_id = {guild_id} order by experience desc limit 20";
            var result = new MySqlCommand(query, con);
            // Console.WriteLine(result);

            MySqlDataReader rdr = result.ExecuteReader();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Title = $"Leaderboard for {guild.Name}",
                Color = guild.Owner.Color,
            };
            embed.AddField("\u200b", "\u200b");


            int position = 0;
            while (rdr.Read())
            {
                var user_id = rdr.GetInt64(0);
                var experience = rdr.GetInt64(2);
                var exp1 = experience.ToString();
                var level = rdr.GetInt64(3);
                position++;
                DiscordMember member = guild.GetMemberAsync((ulong) user_id).Result;
                var user = member.DisplayName + " - (" + member.Username + "#" + member.Discriminator + ")";
                string response =  user + ", Experience: " + experience + ", Level: " + level;

                // embed.AddField("Experience: ", exp1);
                // embed.AddField("User: ", member.DisplayName);
                // embed.AddField("Level: ", level.ToString());
                embed.AddField(position + " - "+ response, "\u200b");

                
                Console.WriteLine(position + ": " + response);
            }

            return embed.Build();
        }

    }
}