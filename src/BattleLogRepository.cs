using System;
using System.Data;
using Npgsql;
using System.Linq;

namespace MTCG;

public class BattleLogRepository : BaseRepository<BattleLogEntry>, IRepository<BattleLogEntry>, IService
{
    public BattleLogRepository()
    : base()
    {
        _Table = "battlelogs";
        _Fields = "id,cardidplayer1,cardidplayer2,player1,player2,roundwinner,battleid,roundnumber";
    }

    public override void Save(BattleLogEntry obj)
    {
        using var builder = new QueryBuilder(Connect());
        builder
            .InsertInto(_Table, _Fields.Split(","))
            .InsertValues(_Fields.Split(",").Select(f => "@" + f).ToArray())
            .AddParam("@id", obj.Id!)
            .AddParam("@player1", obj.Player1!.ID)
            .AddParam("@player2", obj.Player2!.ID)
            .AddParam("@cardidplayer1", obj.CardPlayedPlayer1!.Id)
            .AddParam("@cardidplayer2", obj.CardPlayedPlayer2!.Id)
            .AddParam("@roundwinner", obj.RoundWinner!.ID)
            .AddParam("@battleid", obj.BattleId)
            .AddParam("@roundnumber", obj.RoundNumber)
            .GetInsertedIds(true)
            .Build();
        
        builder.ExecuteNonQuery();
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
        var userRepo = ServiceProvider.GetDisposable<UserRepository>();
        var cardRepo = ServiceProvider.GetDisposable<CardRepository>();

        obj.Id = re.GetGuid(re.GetOrdinal("id"));
        obj.Player1 = userRepo.Get(re.GetGuid(re.GetOrdinal("player1")));
        obj.Player2 = userRepo.Get(re.GetGuid(re.GetOrdinal("player2")));
        obj.CardPlayedPlayer1 = cardRepo.Get(re.GetGuid(re.GetOrdinal("cardidplayer1")));
        obj.CardPlayedPlayer2 = cardRepo.Get(re.GetGuid(re.GetOrdinal("cardidplayer2")));
        obj.RoundWinner = userRepo.Get(re.GetGuid(re.GetOrdinal("roundwinner")));
        obj.TimeStamp = re.GetDateTime(re.GetOrdinal("timestamp"));
        obj.RoundNumber = re.GetInt32(re.GetOrdinal("roundnumber"));
        obj.BattleId = re.GetGuid(re.GetOrdinal("battleid"));
    }

    protected override void _Fill(BattleLogEntry obj, IDataReader re)
    {
        Fill(obj, re);
    }



}