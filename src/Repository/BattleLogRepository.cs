using System;
using System.Data;
using Npgsql;
using System.Linq;

namespace MTCG;

public class BattleLogRepository : BaseRepository<BattleLogEntry>, IRepository<BattleLogEntry>, IService
{
    protected static object battleLogLock = new object();

    public BattleLogRepository()
    : base()
    {
        _Table = "battlelogs";
        _Fields = "id,player1,player2,cardidplayer1,cardidplayer2,roundwinner,battleid,roundnumber,isdraw";
    }

    public override void Save(BattleLogEntry obj)
    {
        lock (battleLogLock)
        {
            try
            {
                using var builder = new QueryBuilder(Connect());
                var fields = _Fields.Split(",").Where(field => field.Trim() != "id").ToArray();
                var winnerIsNull = obj.RoundWinner == null;
                dynamic winner = winnerIsNull ? DBNull.Value : obj.RoundWinner!.ID;

                builder
                    .InsertInto(_Table, fields)
                    .InsertValues(fields.Select(f => "@" + f).ToArray())
                    .AddParam("@player1", obj.Player1!.ID)
                    .AddParam("@player2", obj.Player2!.ID)
                    .AddParam("@cardidplayer1", obj.CardPlayedPlayer1!.Id)
                    .AddParam("@cardidplayer2", obj.CardPlayedPlayer2!.Id);

                if (winnerIsNull)
                    builder.AddParam("@roundwinner", DBNull.Value);
                else
                    builder.AddParam("@roundwinner", obj.RoundWinner!.ID);

                builder
                    .AddParam("@battleid", obj.BattleId!)
                    .AddParam("@roundnumber", obj.RoundNumber)
                    .AddParam("@isdraw", obj.IsDraw)
                    .GetInsertedIds(true)
                    .Build();

                builder.ExecuteNonQuery();
            }
            catch (PostgresException pex)
            {
                if (pex.SqlState == "23505")
                    Console.WriteLine("Battlelog entry already saved.");
                else
                    throw pex;
            }

        }
    }


    public override BattleLogEntry? Get(Guid id)
    {
        return base.Get(id);
    }

    public override void Delete(BattleLogEntry obj)
    {
        using var builder = new QueryBuilder(Connect());
        builder
            .DeleteFrom(_Table)
            .Where("id=@id")
            .AddParam("@id", obj.Id!)
            .Build();
    }

    public override IEnumerable<BattleLogEntry> GetAll()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<BattleLogEntry>? GetAllByBattleId(Guid battleId)
    {
        ObjectBuilder<BattleLogEntry> fill = Fill;
        using var builder = new QueryBuilder(Connect());
        builder
            .Select("*")
            .From(_Table)
            .Where("battleid=@battleId")
            .AddParam("@battleId", battleId)
            .Build();

        return builder.ReadMultiple<BattleLogEntry>(fill) ?? null;
    }

    protected override void Fill(BattleLogEntry obj, IDataReader re)
    {
        var userRepo = ServiceProvider.GetFreshInstance<UserRepository>();
        var cardRepo = ServiceProvider.GetFreshInstance<CardRepository>();

        obj.Id = re.GetGuid(re.GetOrdinal("id"));
        obj.Player1 = userRepo.Get(re.GetGuid(re.GetOrdinal("player1")));
        obj.Player2 = userRepo.Get(re.GetGuid(re.GetOrdinal("player2")));
        obj.CardPlayedPlayer1 = cardRepo.Get(re.GetGuid(re.GetOrdinal("cardidplayer1")));
        obj.CardPlayedPlayer2 = cardRepo.Get(re.GetGuid(re.GetOrdinal("cardidplayer2")));
        obj.RoundWinner = userRepo.Get(re.GetGuid(re.GetOrdinal("roundwinner")));
        obj.TimeStamp = re.GetDateTime(re.GetOrdinal("timestamp"));
        obj.RoundNumber = re.GetInt32(re.GetOrdinal("roundnumber"));
        obj.BattleId = re.GetGuid(re.GetOrdinal("battleid"));
        obj.IsDraw = re.GetBoolean(re.GetOrdinal("isdraw"));
    }

    protected override void _Fill(BattleLogEntry obj, IDataReader re)
    {
        Fill(obj, re);
    }



}