using System;
using System.Data;
using Npgsql;
using System.Linq;

namespace MTCG;

public class UserRepository : BaseRepository<User>, IRepository<User>, IService
{

  protected CardRepository cardRepo = new CardRepository();

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

  // OBSOLETE
  public void SaveSession(User user, string token)
  {
    var builder = new QueryBuilder(Connect());
    builder
      .InsertInto("session")
      .InsertValues("@userid", "@token")
      .AddParam("@userid", user.ID)
      .AddParam("@token", token)
      .Build();

    builder.ExecuteNonQuery();
  }


  //////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////


  public void Update(User user)
  {
    using var builder = new QueryBuilder(Connect());
    builder
      .Update("users")
        .UpdateSet("bio", "@bio")
        .UpdateSet("image", "@image")
        .UpdateSet("name", "@name")
        .UpdateSet("coins", "@coins")
        .UpdateSet("language", "@lang")
      .Where("id=@id")
    .AddParam("@bio", user.Bio)
    .AddParam("@image", user.Image)
    .AddParam("@name", user.Name)
    .AddParam("@id", user.ID)
    .AddParam("@coins", user.Coins)
    .AddParam("@lang", user.Language)
    .Build();

    builder.ExecuteNonQuery();
    SessionManager.UpdateUser(user);
  }


  //////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////


  public override void Delete(User obj)
  {
    using var builder = new QueryBuilder(Connect());
    builder
      .DeleteFrom("users")
      .Where("id=@id")
      .AddParam("@id", obj.ID)
      .Build();

    builder.ExecuteNonQuery();

    SessionManager.EndSession(obj.Token);
  }


  //////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////


  public override IEnumerable<User> GetAll()
  {
    ObjectBuilder<User> objectBuilder = _Fill;
    using var builder = new QueryBuilder(Connect());
    builder
      .Select("u.*", "r.id", "r.role AS rolename")
      .From("users u")
      .Join("roles r")
      .On("r.id=u.role")
      .Build();

    IEnumerable<User> users = builder.ReadMultiple<User>(objectBuilder);

    foreach (var user in users)
      LoadUserCards(user);
  
    return users;
  }


  //////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////


  /// <summary>
  /// CLoads a users stack and deck cards.
  /// </summary>
  /// <param name="user"></param>
  public void LoadUserCards(User? user)
  {
    if (user == null)
      return;

    user.Stack = new List<Card>(cardRepo.GetAllCardsInStackByUserId(user.ID));
    var deckCards = cardRepo.GetDeckByUserId(user.ID);
    user.Deck = deckCards != null ? new List<DeckCard>(deckCards) : new List<DeckCard>();
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
    using var builder = new QueryBuilder(Connect());
    builder
      .Select("u.*", "r.id", "r.role AS rolename")
      .From("users u")
      .Join("roles r")
      .On("r.id=u.role")
      .Where("u.id=@id")
      .AddParam("@id", id)
      .Build();


    User? user = builder.Read<User>(objectBuilder);
    LoadUserCards(user);

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