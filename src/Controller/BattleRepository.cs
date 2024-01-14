using System;
using System.Data;
using Npgsql;
using System.Linq;

namespace MTCG;

public class BattleRepository : BaseRepository<Battle>, IRepository<Battle>, IService
{
    public BattleRepository()
    : base()
    {
        _Table = "battles";
        _Fields = "id,player1,player2,winner,isdraw,enddatetime,countrounds,gainedpoints, battletoken";
    }

    public override Battle? Get(Guid id)
    {
        return base.Get(id);
    }

    public Battle? GetBattleForUser(Guid userId)
    {
        ObjectBuilder<Battle> fill = Fill;
        using var builder = new QueryBuilder(Connect());
        builder
            .Select("*")
            .From(_Table)
            .Where("player1=@userId")
            .Or("player2=@userId")
            .AddParam("@userId", userId)
            .Build();

        return builder.Read<Battle>(fill) ?? null;
    }

    public override void Save(Battle obj)
    {
        var battleLogRepo = ServiceProvider.GetDisposable<BattleLogRepository>();

        var fields = _Fields.Split(",");
        using var builder = new QueryBuilder(Connect());
        builder
            .InsertInto(_Table, fields)
            .InsertValues(fields.Select(f => "@" + f).ToArray())
            .AddParam("@id", obj.Id!)
            .AddParam("@player1", obj.Player1!.ID)
            .AddParam("@player2", obj.Player2!.ID)
            .AddParam("@winner", obj.Winner?.ID)
            .AddParam("@isdraw", obj.IsDraw)
            .AddParam("@enddatetime", obj.EndDateTime)
            .AddParam("@countrounds", obj.CountRounds)
            .AddParam("@gainedpoints", obj.GainedPoints)
            .AddParam("@battletoken", obj.BattleToken)
            .GetInsertedIds(true)
            .Build();

        Guid? insertedId = builder.ReadSingle<Guid>("id");

        if (insertedId == null) throw new Exception("Could not insert Battle");

        foreach (var entry in obj.BattleLog)
        {
            entry.BattleId = insertedId;
            battleLogRepo.Save(entry);
        }
    }

    public override void Delete(Battle obj)
    {
        using var builder = new QueryBuilder(Connect());
        builder
            .DeleteFrom(_Table)
            .Where("id=@id")
            .AddParam("@id", obj.Id!)
            .Build();
    }

    protected override void Fill(Battle obj, IDataReader re)
    {
        var userRepo = ServiceProvider.GetDisposable<UserRepository>();
        var logEntryRepo = ServiceProvider.GetDisposable<BattleLogRepository>();

        obj.Id = re.GetGuid(re.GetOrdinal("id"));
        obj.Player1 = userRepo.Get(re.GetGuid(re.GetOrdinal("player1")));
        obj.Player2 = userRepo.Get(re.GetGuid(re.GetOrdinal("player2")));
        obj.Winner = userRepo.Get(re.GetGuid(re.GetOrdinal("winner"))) ?? null;
        obj.IsDraw = re.GetBoolean(re.GetOrdinal("isdraw"));
        obj.EndDateTime = re.GetDateTime(re.GetOrdinal("enddatetime"));
        obj.CountRounds = re.GetInt32(re.GetOrdinal("countrounds"));
        obj.GainedPoints = re.GetInt32(re.GetOrdinal("gainedpoints"));
        obj.BattleToken = re.GetString(re.GetOrdinal("battletoken"));
        obj.BattleLog = logEntryRepo.GetAllByBattleId(obj.Id.Value)?.ToList() ?? null;
    }

    protected override void _Fill(Battle obj, IDataReader re)
    {
        Fill(obj, re);
    }
}