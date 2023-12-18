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

  public override void Save(User user)
  {

    using (NpgsqlConnection? connection = this._Connect())
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

  public User? GetByName(string username)
  {
    using (NpgsqlConnection? connection = this._Connect())
    using (var command = new NpgsqlCommand($"SELECT * FROM {_Table} WHERE name=@name", connection))
    {
      command.Parameters.AddWithValue("@name", username);

      IDataReader re = command.ExecuteReader();
      User? user = null;

      if (re.Read())
      {
        user = new User();
        _Fill(user, re);
      }

      command.Dispose(); connection!.Dispose();

      return user;
    }
  }



  public override User? Get(Guid id)
  {
    return base.Get(id);
  }


  protected override void _Fill(User user, IDataReader re)
  {
    user.ID = re.GetGuid(re.GetOrdinal("id"));
    user.Name = re.GetString(re.GetOrdinal("name"));
    user.Password = re.GetString(re.GetOrdinal("password"));
    user.Bio = re.GetString(re.GetOrdinal("bio"));
    user.Coins = re.GetInt32(re.GetOrdinal("coins"));
    user.Image = re.GetString(re.GetOrdinal("image"));
  }
}