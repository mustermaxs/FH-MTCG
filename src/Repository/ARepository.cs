using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Npgsql;

namespace MTCG
{
    /// <summary>This class provides a repository base implementation.</summary>
    public abstract class BaseRepository<T> : IRepository<T> where T : IModel, new()
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // protected static members                                                                                         //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Database connection.</summary>
        // protected readonly NpgsqlConnection? connection = _Connect();
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // protected members                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Database table name.</summary>
        protected string _Table = "";

        /// <summary>Table fields list.</summary>
        protected string _Fields = "";


        protected abstract void Fill(T obj, IDataReader re);

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // protected methods                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Sets the data for an object instance.</summary>
        /// <param name="obj">Object.</param>
        /// <param name="re">Database cursor.</param>
        protected abstract void _Fill(T obj, IDataReader re);



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private static methods                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates and returns a valid database connection.</summary>
        /// <returns>Database connection.</returns>
        protected NpgsqlConnection? Connect()
        {
            try
            {
                NpgsqlConnection connection = new NpgsqlConnection("Host=localhost;Username=admin;Password=12345;Database=mtc");

                connection.Open();

                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open connection to DB. {ex}");

                return null;
            }

        }

        public static void closeConnectionToDataBase(IDbConnection connection)
        {
            connection.Close();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // [interface] IRepository<T>                                                                                       //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the object by its ID.</summary>
        /// <param name="id">ID.</param>
        /// <returns>Returns the retrieved object or NULL if there is no object with the given ID..</returns>
        public virtual T? Get(Guid id)
        {
            using (var connection = this.Connect())
            {
                var command = new NpgsqlCommand($"SELECT {_Fields} FROM {_Table} WHERE id = @id", connection);
                T? rval = new();
                command.Parameters.AddWithValue("id", id);

                IDataReader re = command.ExecuteReader();

                if (re.Read())
                {
                    rval = new();
                    _Fill(rval, re);
                }

                re.Close();
                re.Dispose(); command.Dispose();

                return rval;
            }

        }


        /// <summary>Gets all objects.</summary>
        /// <returns>List of objects.</returns>
        /// 12.04.2023 15:11
        /// REFACTOR
        public virtual IEnumerable<T> GetAll()
        {
            using (var connection = this.Connect())
            using (var command = new NpgsqlCommand($"SELECT * FROM {_Table}", connection))
            {
                Dictionary<string, string> dbParams = new Dictionary<string, string>();
                List<T> rval = new();

                IDataReader re = command.ExecuteReader();

                while (re.Read())
                {
                    T v = new();
                    _Fill(v, re);
                    rval.Add(v);
                }

                re.Close();
                re.Dispose();

                return rval;
            }
        }


        /// <summary>Delets an object.</summary>
        /// <param name="obj">Object.</param>
        public virtual void Delete(T obj)
        {
            using (var connection = this.Connect())
            {
                IDbCommand cmd = connection.CreateCommand();
                cmd.CommandText = $"SELECT {_Fields} FROM {_Table}";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }

        }


        /// <summary>Saves an object.</summary>
        /// <param name="obj">Object.</param>
        public abstract void Save(T obj);
    }
}