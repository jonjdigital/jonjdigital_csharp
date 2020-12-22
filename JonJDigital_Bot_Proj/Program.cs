﻿using System;
using System.Threading.Tasks;
using DSharpPlus;

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
                var jonjdigital = 229244232064434177;
                var jonjdigital_test = 691346983494877216;

                // test1.helloWorld(e.Message);
                
                //get basic message details
                string msg = e.Message.Content;
                int author = (int) e.Message.Author.Id;

                var response = test1.levelUp(e.Message);
                if (response != "")
                {
                    e.Message.RespondAsync(response);
                }

                //tester msg response
                /*if (msg.ToLower().StartsWith(prefix + "hello"))
                {
                    await e.Message.RespondAsync(("Hello <@" + e.Message.Author.Id) + ">!");
                }*/
                
                //public commands
                if (msg.ToLower() == prefix + "ping")
                {
                    if (author == jonjdigital || author == jonjdigital_test)
                    {
                        // PublicCmd cmd = new PublicCmd();
                        // Console.WriteLine(test1.ping());
                        e.Message.RespondAsync(test1.ping());
                        //await e.Message.RespondAsync("Hello <@" + e.Message.Author.Id + ">!");
                    }else if (author != jonjdigital && author != jonjdigital_test)
                    {
                        e.Message.RespondAsync("Sorry <@" + e.Message.Author.Id + ">, but this is Developer only command!");

                    }

                    // Console.WriteLine();
                }

            };
            
            await Discord.ConnectAsync();
            await Task.Delay(-1);

        }
    }
}