using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using RconArkLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using DataAccess.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DataAccess.Services;

namespace KickMe.Commands
{
    public class KickCommand : BaseCommandModule
    {
        private readonly RconArkClient Rcon;
        private readonly Config Config;
        private readonly Random Random;
        public KickCommand(RconArkClient rconArkClient, Config config, Random random)
        {
            Rcon = rconArkClient;
            Config = config;
            Random = random;
        }


        [Command("kickme2")]
        public async Task KickMe(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var steamId = await GetVerifiedUser(ctx.Member.Id);
            if (steamId == null)
            {
                await ctx.RespondAsync("You need to be verified to use this command");
                return;
            }

            var user = await Rcon.GetUserAsync(Convert.ToUInt64(steamId));
            if (Config.KickNoCheck == true)
            {
                Console.WriteLine("kicking with no check");
                await user.KickNoCheck();
            }
            else
            {
                var result = await user.KickIfOnline();
                if (result == 1)
                {
                    await ctx.RespondAsync("You need to be ingame to use this command");
                    return;
                }
            }

            var random = Random.Next(Config.Gifs.Count);
            var gif = Config.Gifs[random];

            await ctx.RespondAsync($"**You got kicked!**");
            await ctx.RespondAsync(gif);
        }


        public async Task<ulong?> GetVerifiedUser(ulong discordId)
        {
            using var dbContext = new ArkContext();

            switch (Config.VerificationPlugin)
            {
                case "Fakka":
                    var fakka = await dbContext.Discordaddonplayers.FirstOrDefaultAsync(x => x.Discid == discordId.ToString());
                    if (fakka == null) return null;
                    return Convert.ToUInt64(fakka.SteamId);

                case "Wooly":
                    var wooly = await dbContext.DiscordIntegrationPlayers.FirstOrDefaultAsync(x => x.DiscordId == Convert.ToInt64(discordId));
                    if (wooly == null) return null;
                    return Convert.ToUInt64(wooly.Steamid);

                default:
                    return null;
            }
        }
    }
}
