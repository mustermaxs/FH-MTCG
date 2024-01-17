using System;
using System.Data;
using Npgsql;
using System.Linq;

namespace MTCG;

public class BattleRepository : BaseRepository<Battle>, IRepository<Battle>, IService
{
    protected static object battleLock = new object();
    public BattleRepository()
    : base()
    {
        _Table = "battles";
        _Fields = "id,player1,player2,winner,isdraw,enddatetime,countrounds,gainedpoints,battletoken";
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public override Battle? Get(Guid id)
    {
        return base.Get(id);
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public override IEnumerable<Battle> GetAll()
    {
        ObjectBuilder<Battle> fill = Fill;
        using var builder = new QueryBuilder(Connect());
        builder
            .Select("*")
            .From(_Table)
            .Build();

        IEnumerable<Battle>? battles = builder.ReadMultiple<Battle>(fill);

        return battles;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



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


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    public override void Save(Battle obj)
    {
        lock (battleLock)
        {
            try
            {
                var battleLogRepo = ServiceProvider.GetFreshInstance<BattleLogRepository>();
                var winnerIsNull = obj.Winner == null;

                dynamic winner = winnerIsNull ? DBNull.Value : obj.Winner!.ID;
                var fields = _Fields.Split(",").Where(field => field.Trim() != "id").ToArray();

                using var builder = new QueryBuilder(Connect());
                builder
                    .InsertInto(_Table, fields)
                    .InsertValues(fields.Select(f => "@" + f).ToArray())
                    .AddParam("@player1", obj.Player1!.ID)
                    .AddParam("@player2", obj.Player2!.ID);

                if (winnerIsNull)
                    builder.AddParam("@winner", DBNull.Value);
                else
                    builder.AddParam("@winner", obj.Winner!.ID);

                builder
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

            catch (PostgresException pex)
            {
                if (pex.SqlState == "23505")
                    Console.WriteLine("Battle entry already saved.");
                else
                    throw pex;
            }
        }
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public Dictionary<string, dynamic> GetUserBattleStats(Guid userId)
    {
        ObjectBuilder<Dictionary<string, dynamic>> fill = (Dictionary<string, dynamic> dict, IDataReader re) =>
        {
            dict.Add("Name", re.GetString(re.GetOrdinal("Name")));
            dict.Add("Elo", re.GetInt32(re.GetOrdinal("Elo")));
            dict.Add("Won", re.GetInt32(re.GetOrdinal("Wins")));
            dict.Add("Lost", re.GetInt32(re.GetOrdinal("Losses")));
        };

        using var builder = new QueryBuilder(Connect());
        builder
            .RawQuery(@"
            SELECT
            u.name as Name, u.elo as Elo, u.id,
            SUM(CASE WHEN b.winner = u.id THEN 1 ELSE 0 END) AS Wins,
            SUM(CASE WHEN b.winner != u.id AND b.winner IS NOT NULL THEN 1 ELSE 0 END) AS Losses
            FROM
                users u
            LEFT JOIN
                battles b ON u.id IN (b.player1, b.player2)
            WHERE
                u.id=@userid
            GROUP BY
                u.id;").AddParam("@userid", userId).Build();

        return builder.Read(fill);
    }


    public List<Dictionary<string, dynamic>> GetAllStats()
    {
        List<Dictionary<string, dynamic>> stats = new List<Dictionary<string, dynamic>>();
        var userRepo = ServiceProvider.GetFreshInstance<UserRepository>();
        var users = userRepo.GetAll();

        foreach (var user in users)
            stats.Add(GetUserBattleStats(user.ID));

        return stats.OrderByDescending(stat => stat["Elo"]).ToList();
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    public override void Delete(Battle obj)
    {
        using var builder = new QueryBuilder(Connect());
        builder
            .DeleteFrom(_Table)
            .Where("id=@id")
            .AddParam("@id", obj.Id!)
            .Build();
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public void DeleteAll()
    {
        using var builder = new QueryBuilder(Connect());
        builder
            .DeleteFrom(_Table)
            .Build();

        builder.ExecuteNonQuery();
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    protected override void Fill(Battle obj, IDataReader re)
    {
        var userRepo = ServiceProvider.GetFreshInstance<UserRepository>();
        var logEntryRepo = ServiceProvider.GetFreshInstance<BattleLogRepository>();

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


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////

    protected override void _Fill(Battle obj, IDataReader re)
    {
        Fill(obj, re);
    }
}