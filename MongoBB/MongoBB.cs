using BBRAPIModules;
using BattleBitAPI.Common;
using MongoModule;
using System.Threading.Tasks;
using System;
namespace MongoBB
{
    public class MongoBB : BattleBitModule
    {
        public MongoBBConfiguration Configuration { get; set; }
        public DataDAO mongodata;
        public override void OnModulesLoaded()
        {
            mongodata = new DataDAO(Configuration.ConnectionString, Configuration.DatabaseName);

        }

        public override Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args)
        {
            Console.WriteLine("Loaded player data for " + steamID);
            Task.Run(async () =>
            {
                args.Stats = (await mongodata.playerDAO.GetPlayer(steamID, args.Stats)).stats;
            });
            return Task.CompletedTask;
        }
        public override Task OnSavePlayerStats(ulong steamID, PlayerStats stats)
        {
            Task.Run(async () =>
            {
                var player = await mongodata.playerDAO.GetPlayer(steamID);

                PlayerStatus status = player.status;

                await mongodata.playerDAO.SavePlayer(steamID, stats, status);

                Console.WriteLine("Saved Player " + steamID);
            });    
            return Task.CompletedTask;
        }
        public override Task OnPlayerConnected(RunnerPlayer player)
        {
            Task.Run(async () =>
            {
                var db = await mongodata.playerDAO.GetPlayer(player.SteamID);
                if (db.status.IsBanned)
                {
                    player.Kick("Banned from server");
                }
            });
            return Task.CompletedTask;
        }
        public class MongoPlayer
        {
            public RunnerPlayer Player { get; set; }
            public MongoPlayer(RunnerPlayer player)
            {
                this.Player = player;
            }


        }
    }
    public class MongoBBConfiguration : ModuleConfiguration
    {
        public string DatabaseName { get; set; } = "CommunityServerAPI";
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    }
}
