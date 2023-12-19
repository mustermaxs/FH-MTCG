using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace MTCG;
// public delegate void FillMethod<T>(T obj, IDataReader reader);
public delegate void ObjectBuilder<T>(T obj, IDataReader reader);

public class QueryBuilder
{
    private string queryString = string.Empty;
    private NpgsqlConnection? _connection = null;
    private Dictionary<string, dynamic> paramMappings = new Dictionary<string, dynamic>();

    public QueryBuilder(NpgsqlConnection? connection)
    {
        if (connection == null) throw new ArgumentNullException(nameof(connection));

        this._connection = connection;
    }

    public QueryBuilder InsertInto(string tableName, string[] fields)
    {
        queryString += " INSERT INTO {tableName} ({Columns(fields)}) ";

        return this;
    }

    public void Reset()
    {
        queryString = string.Empty;
        paramMappings.Clear();
    }

    public string Build()
    {
        return queryString;
    }

    public QueryBuilder Select(string[] columns)
    {
        queryString = queryString + $" SELECT {Columns(columns)} ";

        return this;
    }
    public QueryBuilder Select(string query)
    {
        queryString = queryString + $" SELECT {query} ";

        return this;
    }

    public string Columns(string[] columns)
    {
        int columnsLength = columns.Length;
        string columnString = string.Empty;

        for (int i = 0; i < columnsLength - 1; i++)
        {
            columnString += columns[i] + ", ";
        }

        columnString += columns[columnsLength - 1];

        return columnString;
    }

    public QueryBuilder Join(string table)
    {
        queryString += $" JOIN {table}";

        return this;
    }

    public QueryBuilder On(string joiningColumns)
    {
        queryString += $" ON {joiningColumns}";

        return this;
    }

    public QueryBuilder From(string table)
    {
        queryString += $" FROM {table}";

        return this;
    }

    public QueryBuilder Where(string condition)
    {
        queryString += $" WHERE {condition}";

        return this;
    }

    public QueryBuilder And(string condition)
    {
        queryString += $" AND {condition}";

        return this;
    }

    public QueryBuilder Or(string condition)
    {
        queryString += $" OR {condition}";

        return this;
    }


    private bool ConnectionIsSet()
    {
        return this._connection != null;
    }

    private void AddParamsWithValue(Dictionary<string, string> paramMappings)
    {
        foreach (var mapping in paramMappings)
        {
            // comman
        }
    }


    public QueryBuilder OrderBy(string column)
    {
        queryString += $" ORDER BY {column}";

        return this;
    }

    public IEnumerable<T> ReadMultiple<T>(ObjectBuilder<T> callback) where T : new()
    {
        // _connection.Open();

        using var command = new NpgsqlCommand(queryString, _connection);

        if (paramMappings.Count > 0)
        {
            foreach (var mapping in paramMappings)
            {
                string key = mapping.Key[0] == '@' ? mapping.Key : '@' + mapping.Key;
                command.Parameters.AddWithValue(key, mapping.Value);
            }
        }

        using var reader = command.ExecuteReader();
        List<T> list = new();

        while (reader.Read())
        {
            T obj = new();
            callback(obj, reader);
            list.Add(obj);
        }

        return list;
    }
    public IEnumerable<T> ReadMultiple<T>(ObjectBuilder<T> callback, NpgsqlCommand command) where T : new()
    {
        // _connection.Open();

        if (paramMappings.Count > 0)
        {
            foreach (var mapping in paramMappings)
            {
                string key = mapping.Key[0] == '@' ? mapping.Key : '@' + mapping.Key;
                command.Parameters.AddWithValue(key, mapping.Value);
            }
        }

        using var reader = command.ExecuteReader();
        List<T> list = new();

        while (reader.Read())
        {
            T obj = new();
            callback(obj, reader);
            list.Add(obj);
        }

        return list;
    }

    public QueryBuilder AddParam(string key, dynamic value)
    {
        paramMappings.Add(key, value);

        return this;
    }

    public int ExecuteNonQuery(NpgsqlCommand command)
    {
        return command.ExecuteNonQuery();
    }


    public int ExecuteNonQuery()
    {
        return command.ExecuteNonQuery();
    }


    protected void CloseConnection()
    {
        throw new NotImplementedException("");
    }

    public IEnumerable<T> Run<T>(ObjectBuilder<T> builderDelegate) where T : new()
    {
        if (queryString == string.Empty) throw new Exception("Invalid query provided.");

        using (var command = new NpgsqlCommand(queryString, _connection))
        {
            return ReadMultiple<T>(builderDelegate);
        }
    }
    public int Run()
    {
        if (queryString == string.Empty) throw new Exception("Invalid query provided.");

        using (var command = new NpgsqlCommand(queryString, _connection))
        {
            return ExecuteNonQuery()
        }
    }

    public int Run()
    {

    }

    public T Read<T>(ObjectBuilder<T> callback) where T : new()
    {
        // _connection.Open();

        using var command = new NpgsqlCommand(queryString, _connection);

        foreach (var mapping in paramMappings)
        {
            string key = mapping.Key[0] == '@' ? mapping.Key : '@' + mapping.Key;
            command.Parameters.AddWithValue(key, mapping.Value);
        }

        using var reader = command.ExecuteReader();
        T obj = new();

        if (reader.Read())
        {
            callback(obj, reader);
        }

        return obj;
    }
}