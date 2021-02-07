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

        private int modCommands = 10;
        private int profileModCommands = 9;
        private int userCommands = 7;

        //MUST HAVE CHANNEL ADMIN ROLE
        /*public DiscordEmbed addUserLog(DiscordChannel channel, DiscordGuild guild, DiscordMessage msg, DiscordMember member)
        {
            if (member.PermissionsIn(msg.Channel).HasPermission(Permissions.ManageChannels))
            {
                
            }
        }*/
        
    }
}