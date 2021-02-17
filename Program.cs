using KickMe.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RconArkLibrary;
using RconArkLibrary.Models;
using DataAccess.Models;
using DataAccess.Services;

namespace KickMe
{
    public class Program
    {
        private DiscordClient Bot;
        private CommandsNextExtension Commands;
        private FileManager FileManager;
        private Config Config;
        private Random Random;
        private RconArkClient Rcon;

        static void Main(string[] args)
        {
            var program = new Program();
            program.MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            FileManager = new FileManager();
            Random = new Random();
            Config = FileManager.GetConfig();

            //rcon client setup
            Rcon = new RconArkClient();
            foreach (Config_Server element in Config.Servers)
            {
                await Rcon.AddConnection(element.Name, element.Address, element.RconPort, element.Password, element.Timeout);
            }

            //bot setup configuration
            Bot = new DiscordClient(new DiscordConfiguration()
            {
                Token = Config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,

            });

            var services = ConfigureServices();

            //commands setup configuration
            var ccfg = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { Config.Prefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                Services = services,
                EnableDefaultHelp = false
            };

            Commands = Bot.UseCommandsNext(ccfg);

            //register commands
            Commands.RegisterCommands<KickCommand>();

            //await services.GetRequiredService<KickManager>().InitializeAsync();

            //login
            await Bot.ConnectAsync();
            await Task.Delay(Timeout.Infinite);

        }


        //dependency injection
        public IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<FileManager>()
                .AddDbContext<ArkContext>()
                .AddSingleton(Rcon)
                .AddSingleton(Random)
                .AddSingleton(Bot)
                .AddSingleton(Config)
                .BuildServiceProvider();
        }
    }
}
