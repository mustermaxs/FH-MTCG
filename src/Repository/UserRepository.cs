using System;
using System.Data;
using Npgsql;

namespace MTCG;

public class UserRepository : BaseRepository<User>, IRepository<User>
{
  public UserRepository()
  : base()
  {
    _Table = "users";
    _Fields = "id, name, password, coins, bio, image, role";
  }


  //////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////


  public override void Save(User user)
  {

    using (NpgsqlConnection? connection = this.Connect())
    using (var command = new NpgsqlCommand($"INSERT INTO {_Table} (name, password, coins, bio, image) VALUES (@name, @password, @coins, @bio, @image);", connection))
    {
      command.Parameters.AddWithValue("@name", user.Name);
      command.Parameters.AddWithValue("@password", user.Password);
      command.Parameters.AddWithValue("@coins", user.Coins);
      command.Parameters.AddWithValue("@bio", user.Bio);
      command.Parameters.AddWithValue("@image", user.Image);

      command.ExecuteNonQuery();

      command.Dispose(); connection!.Dispose();
    }
  }


  //////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////


  public void Update(User user)
  {
    var builder = new QueryBuilder(Connect());
    builder
      .Update("users")
        .UpdateSet("bio", "@bio")
        .UpdateSet("image", "@image")
        .UpdateSet("name", "@name")
        .UpdateSet("language", "@lang")
      .Where("id=@id")
    .AddParam("@bio", user.Bio)
    .AddParam("@image", user.Image)
    .AddParam("@name", user.Name)
    .AddParam("@id", user.ID)
    .AddParam("@lang", user.Language)
    .Build();

    builder.ExecuteNonQuery();
  }


  //////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////


  public override void Delete(User obj)
  {
    var builder = new QueryBuilder(Connect());
    builder
      .DeleteFrom("users")
      .Where("id=@id")
      .AddParam("@id", obj.ID)
      .Build();

    builder.ExecuteNonQuery();
  }


  //////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////



  public User? GetByName(string username)
  {
    ObjectBuilder<User> objectBuilder = _Fill;
    using var builder = new QueryBuilder(Connect());
    builder
      .Select("u.*", "r.id", "r.role AS rolename")
      .From("users u")
      .Join("roles r")
      .On("r.id=u.role")
      .Where("u.name=@name")
      .AddParam("@name", username)
      .Build();

    User? user = builder.Read<User>(objectBuilder);

    return user ?? null;
  }



  //////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////



  public override User? Get(Guid id)
  {
    ObjectBuilder<User> objectBuilder = _Fill;
    var builder = new QueryBuilder(Connect());
    builder
      .Select("u.*", "r.id", "r.role AS rolename")
      .From("users u")
      .Join("roles r")
      .On("r.id=u.role")
      .Where("id=@id")
      .AddParam("@id", id)
      .Build();

    User? user = builder.Read<User>(objectBuilder);

    return user ?? null;
  }


  //////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////


  protected override void Fill(User obj, IDataReader re)
  {
    throw new NotImplementedException();
  }


  //////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////


  protected override void _Fill(User user, IDataReader re)
  {
    user.ID = re.GetGuid(re.GetOrdinal("id"));
    user.Name = re.GetString(re.GetOrdinal("name"));
    user.Password = re.GetString(re.GetOrdinal("password"));
    user.Bio = re.GetString(re.GetOrdinal("bio"));
    user.Coins = re.GetInt32(re.GetOrdinal("coins"));
    user.Image = re.GetString(re.GetOrdinal("image"));
    user.Language = re.GetString(re.GetOrdinal("language"));
    user.UserAccessLevel = Enum.TryParse<Role>(re.GetString(re.GetOrdinal("rolename")), out Role role) ? role : Role.ANONYMOUS;
  }
}