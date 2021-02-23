using System;
using DSharpPlus;
using DSharpPlus.Entities;
using MySql.Data.MySqlClient;

namespace JonJDigital_Bot_Proj
{
    public class ModLog
    {
        private static string conStr = Environment.GetEnvironmentVariable("DISCORD_MYSQL");
        private MySqlConnection con = new MySqlConnection(conStr);

        private DiscordEmbedBuilder emb = new DiscordEmbedBuilder();
        
        private int modCommands = 10;
        // private int profileModCommands = 9;
        private int userCommands = 7;

        //MUST HAVE CHANNEL ADMIN ROLE
        public DiscordEmbed addLogChannel(DiscordChannel channel, DiscordMessage msg, DiscordUser user)
        {
            DiscordMember member = channel.Guild.GetMemberAsync(user.Id).Result; 

            emb.Title = "Logger Channel";

            if (member.PermissionsIn(msg.Channel).HasPermission(Permissions.ManageChannels))
            {
                con.Open();
                string stm =
                    $"select * from log_channels where guild_id={channel.GuildId} and log_type = {modCommands}";
                MySqlCommand cmd = new MySqlCommand(stm, con);
                MySqlDataReader rdr = cmd.ExecuteReader();
                
                //if log channel exists update it
                if (rdr.Read())
                {
                    rdr.Close();
                    stm =
                        $"update log_channels set channel_id = {channel.Id} where guild_id={channel.GuildId} and log_type = {modCommands}";
                    cmd = new MySqlCommand(stm, con);
                    rdr = cmd.ExecuteReader();
                    if (!rdr.RecordsAffected.Equals(1))
                    {
                        Console.WriteLine(stm);
                    }
                    else
                    {
                        emb.AddField($"Mod log channel set to {channel.Name} for the server {channel.Guild.Name}", "\u200b");
                    }
                }
                else
                {
                    rdr.Close();
                    stm =
                        $"insert into log_channels(guild_id, channel_id, log_type) values({channel.GuildId},{channel.Id},{modCommands})";
                    cmd = new MySqlCommand(stm, con);
                    rdr = cmd.ExecuteReader();
                    if (rdr.RecordsAffected.Equals(1))
                    {
                        emb.AddField($"Mod log channel set to {channel.Name} for the server {channel.Guild.Name} ","\u200b");
                    }
                }
            }
            else
            {
                emb.AddField($"{member.DisplayName} You do not have permission to modify the log channel", "\u200b");
            }

            emb.WithFooter(member.Username + "#" + member.Discriminator, member.AvatarUrl);
            return emb.Build();
        }
        
        public DiscordEmbed addUserLogChannel(DiscordChannel channel, DiscordMessage msg, DiscordUser user)
        {
            DiscordMember member = channel.Guild.GetMemberAsync(user.Id).Result; 

            emb.Title = "User Logger Channel";

            if (member.PermissionsIn(msg.Channel).HasPermission(Permissions.ManageChannels))
            {
                con.Open();
                string stm =
                    $"select * from log_channels where guild_id={channel.GuildId} and log_type = {userCommands}";
                MySqlCommand cmd = new MySqlCommand(stm, con);
                MySqlDataReader rdr = cmd.ExecuteReader();
                
                //if log channel exists update it
                if (rdr.Read())
                {
                    rdr.Close();
                    stm =
                        $"update log_channels set channel_id = {channel.Id} where guild_id={channel.GuildId} and log_type = {userCommands}";
                    cmd = new MySqlCommand(stm, con);
                    rdr = cmd.ExecuteReader();
                    if (!rdr.RecordsAffected.Equals(1))
                    {
                        Console.WriteLine(stm);
                    }
                    else
                    {
                        emb.AddField($"User log channel set to {channel.Name} for the server {channel.Guild.Name}", "\u200b");
                    }
                }
                else
                {
                    rdr.Close();
                    stm =
                        $"insert into log_channels(guild_id, channel_id, log_type) values({channel.GuildId},{channel.Id},{userCommands})";
                    cmd = new MySqlCommand(stm, con);
                    rdr = cmd.ExecuteReader();
                    if (rdr.RecordsAffected.Equals(1))
                    {
                        emb.AddField($"User log channel set to {channel.Name} for the server {channel.Guild.Name} ","\u200b");
                    }
                }
            }
            else
            {
                emb.AddField($"{member.DisplayName} You do not have permission to modify the log channel", "\u200b");
            }

            emb.WithFooter(member.Username + "#" + member.Discriminator, member.AvatarUrl);
            return emb.Build();
        }

        public DiscordEmbed removeLogChannel(DiscordMessage msg, DiscordUser user)
        {
            
            emb.Title = "Logger Channel";
            DiscordMember member = msg.Channel.Guild.GetMemberAsync(user.Id).Result;
            if (member.PermissionsIn(msg.Channel).HasPermission(Permissions.ManageChannels))
            {
                string stm =
                    $"delete from log_channels where guild_id={msg.Channel.GuildId} and log_type={modCommands}";
                con.Open();
                MySqlCommand cmd = new MySqlCommand(stm, con);
                MySqlDataReader rdr = cmd.ExecuteReader();

                
                if (rdr.RecordsAffected.Equals(1))
                {
                    emb.AddField("\u200b", $"User Log channel removed for server {msg.Channel.Guild.Name}");
                }

                emb.WithFooter(user.Username + "#" + user.Discriminator, user.AvatarUrl);
            }

            else
            {
                emb.AddField("\u200b", $"You need to have the Manage Channels Permission to use this command.");

            }
            emb.WithFooter(user.Username + "#" + user.Discriminator, user.AvatarUrl);
            return emb.Build();
        }
    }
}