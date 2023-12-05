using System;
using System.Data;
using Npgsql;

namespace MTCG;

public class UserRepository : BaseRepository<User>, IRepository<User>
{
  public UserRepository()
  :base()
  {
    _Table = "users";
    _Fields = "id, name, password, coins, bio, image";
  }

  public override void Save(User user)
  {
    try
    {
      using(NpgsqlConnection? connection = this._Connect())
      using (var command = new NpgsqlCommand($"INSERT INTO {_Table} (name, password, coins, bio, image) VALUES (@name, @password, @coins, @bio, @image);", connection))
      {
        command.Parameters.AddWithValue("@name", user.Name);
        command.Parameters.AddWithValue("@password", user.Password);
        command.Parameters.AddWithValue("@coins", user.Coins);
        command.Parameters.AddWithValue("@bio", user.Bio);
        command.Parameters.AddWithValue("@image", user.Image);

        command.ExecuteNonQuery();

        command.Dispose(); connection.Dispose();
      }
    }
    catch (Exception ex)
    {
      throw new Exception($"Failed to register new user");
    }
  }

  public override User? Get(int id)
  {
    return base.Get(id);
  }


  protected override void _Fill(User user, IDataReader re)
  {
    user.ID = re.GetInt32(re.GetOrdinal("id"));
    user.Name = re.GetString(re.GetOrdinal("name"));
    user.Password = re.GetString(re.GetOrdinal("password"));
    user.Bio = re.GetString(re.GetOrdinal("bio"));
    user.Coins = re.GetInt32(re.GetOrdinal("coins"));
    user.Image = re.GetString(re.GetOrdinal("image"));
  }
}