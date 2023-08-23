using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BattleBitAPI.Common;
using MongoDB.Bson;
using MongoDB.Driver;
namespace MongoModule
{


    public class PlayerDAO
    {
        private readonly IMongoCollection<PlayerSave> _playerCollection;

        public PlayerDAO(IMongoDatabase mongoDatabase)
        {
            _playerCollection = mongoDatabase.GetCollection<PlayerSave>("players");
        }

        public async Task<List<PlayerSave>> GetAsync() =>
            await _playerCollection.Find(_ => true).ToListAsync();

        public async Task<PlayerSave> GetAsync(string id) =>
            await _playerCollection.Find(x => x.Id.ToString().Equals(id)).FirstOrDefaultAsync();

        public async Task<PlayerSave> GetBySteamIDAsync(ulong id) =>
            await _playerCollection.Find(x => x.steamid.Equals(id)).FirstOrDefaultAsync();

        public async Task CreateAsync(PlayerSave newplayer) =>
            await _playerCollection.InsertOneAsync(newplayer);

        public async Task<PlayerSave> GetPlayer(ulong steamid)
        {
            var player = await _playerCollection.Find(x => x.steamid.ToString().Equals(steamid.ToString())).FirstOrDefaultAsync();
            if (player is null)
            {
                var newplayer = new PlayerSave(steamid, new PlayerStats());
                await CreateAsync(newplayer);
                player = newplayer;
            }
            return player;
        }
        public async Task<PlayerSave> GetPlayer(ulong steamid, PlayerStats stats)
        {
            var player = await _playerCollection.Find(x => x.steamid.ToString().Equals(steamid.ToString())).FirstOrDefaultAsync();
            if (player is null)
            {
                var newplayer = new PlayerSave(steamid, stats);
                await CreateAsync(newplayer);
                player = newplayer;
            }
            return player;
        }
        public async Task SavePlayer(ulong steamid, PlayerStats stats, PlayerStatus status)
        {
            var player = await _playerCollection.Find(x => x.steamid.ToString().Equals(steamid.ToString())).FirstOrDefaultAsync();
            player.stats = stats;
            player.status = status;
            await UpdateAsync(player.Id, player);
        }
        public async Task UpdateAsync(ObjectId id, PlayerSave updatedUser) =>
            await _playerCollection.ReplaceOneAsync(x => x.Id.Equals(id), updatedUser);

    }
    public class DataDAO
    {
        public PlayerDAO playerDAO;

        public DataDAO(string ConnectionString, string DatabaseName)
        {
            var mongoClient = new MongoClient(ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(DatabaseName);
            if (mongoDatabase == null)
            {
                Console.WriteLine("Check your mongodb configuration");
            }
            Console.WriteLine("MongoDB Connected Successfully");
            playerDAO = new PlayerDAO(mongoDatabase);
        }
    }
    public class PlayerSave
    {
        public ObjectId Id { get; set; }
        public ulong steamid { get; set; }
        public PlayerStatus status = new PlayerStatus();
        public PlayerStats stats { get; set; }

        public PlayerSave(ulong steamid, PlayerStats stats)
        {
            this.steamid = steamid;
            this.stats = stats;
        }
    }
    public class PlayerStatus
    {
        public bool IsBanned = false;
    }
}
