using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace MTCG;
// public delegate void FillMethod<T>(T obj, IDataReader reader);

public enum QBCommand
{
    SELECT,
    INSERT,
    WHERE,
    ON,
    UPDATE,
    SET,
    JOIN,
    FROM,
    RAW_QUERY,
    AND,
    OR,
    IN,
    ADD_PARAM,
    ORDERBY,
    INSERT_VALUES,
    VALUES,
    ADD_INSERT_VALUE
}

public delegate void ObjectBuilder<T>(T obj, IDataReader reader);

// public class QBParam<T>
// {
//     public string Name { get; set; }
//     public T Value { get; set; }
//     public int Index { get; set; }
// }

public class QueryBuilder
{
    private string queryString = string.Empty;
    protected bool returnInsertedId;
    protected bool insertedValues = false;
    protected bool isInsertStatement = false;
    private NpgsqlConnection? _connection = null;
    protected bool calledBuild = false;
    private Dictionary<string, dynamic> paramMappings = new Dictionary<string, dynamic>();
    protected List<QBCommand> commandSequence = new();
    protected Dictionary<int, (int Index, string Key, dynamic Value)> paramSequence = new();
    protected Dictionary<int, string> columnSequence = new();
    // protected Dictionary<int,
    protected int commandIndex = 0;
    protected int qsIndex = 0;
    protected List<string> qString = new();
    protected Dictionary<QBCommand, string> commandStringMappings = new()
    {
        { QBCommand.SELECT, " SELECT" },
        { QBCommand.INSERT, " INSERT INTO " },
        { QBCommand.WHERE, " WHERE" },
        { QBCommand.ON, " ON" },
        { QBCommand.UPDATE, " UPDATE" },
        { QBCommand.SET, " SET" },
        { QBCommand.JOIN, " JOIN" },
        { QBCommand.FROM, " FROM" },
        { QBCommand.RAW_QUERY, "STRING_QUERY" },
        { QBCommand.AND, " AND" },
        { QBCommand.OR, " OR" },
        { QBCommand.IN, " IN" },
        { QBCommand.ORDERBY, " ORDER BY" },
        { QBCommand.VALUES, " VALUES" }
    };

    public QueryBuilder(NpgsqlConnection? connection)
    {
        if (connection == null) throw new ArgumentNullException(nameof(connection));

        this._connection = connection;
    }

    public QueryBuilder InsertInto(string tableName, string[] fields)
    {
        isInsertStatement = true;
        commandSequence.Add(QBCommand.INSERT);
        queryString += " {tableName} ({Columns(fields)}) ";
        commandIndex++;

        return this;
    }

    public QueryBuilder RawQuery(string query)
    {
        commandSequence.Add(QBCommand.RAW_QUERY);
        columnSequence[commandIndex] = query;
        commandIndex++;

        return this;
    }


    public void Reset()
    {
        calledBuild = false;
        qString = new();
        commandIndex = 0;
        commandSequence = new();
        columnSequence = new();
        paramSequence = new();
        queryString = string.Empty;
        paramMappings.Clear();
    }


    public QueryBuilder Select(IEnumerable<string> columns)
    {
        commandSequence.Add(QBCommand.SELECT);
        columnSequence[commandIndex] = Columns(columns);
        commandIndex++;

        return this;
    }
    public QueryBuilder Select(params string[] columns)
    {
        commandSequence.Add(QBCommand.SELECT);
        columnSequence[commandIndex] = Columns(columns);
        commandIndex++;

        return this;
    }

    public string Columns(IEnumerable<string> columns)
    {
        int columnsLength = columns.Count();
        string columnString = string.Empty;

        string[] columnsArray = columns.ToArray();

        for (int i = 0; i < columnsLength - 1; i++)
        {
            qsIndex++;
            columnString += columnsArray[i] + ", ";
        }

        columnString += columnsArray[columnsLength - 1];

        return $" {columnString} ";
    }
    public string Columns(params string[] columns)
    {
        int columnsLength = columns.Length;
        string columnString = string.Empty;

        for (int i = 0; i < columnsLength - 1; i++)
        {
            qsIndex++;
            columnString += columns[i] + ", ";
        }

        columnString += columns[columnsLength - 1];

        return $" {columnString} ";
    }

    public QueryBuilder Values(string[] columns)
    {
        commandSequence.Add(QBCommand.VALUES);
        columnSequence[commandIndex] = $" ({Columns(columns)}) ";
        commandIndex++;

        return this;
    }
    public QueryBuilder Values(IEnumerable<string> columns)
    {
        commandSequence.Add(QBCommand.VALUES);
        columnSequence[commandIndex] = $" ({Columns(columns)}) ";
        commandIndex++;

        return this;
    }

    public QueryBuilder Join(string table)
    {
        commandSequence.Add(QBCommand.JOIN);
        columnSequence[commandIndex] = Columns(table.Split(","));
        commandIndex++;

        return this;
    }

    public QueryBuilder On(string joiningColumns)
    {
        commandSequence.Add(QBCommand.ON);
        columnSequence[commandIndex] = $" {joiningColumns} ";

        commandIndex++;

        return this;
    }

    public QueryBuilder From(string table)
    {
        commandSequence.Add(QBCommand.FROM);
        columnSequence[commandIndex] = $" {table} ";
        commandIndex++;

        return this;
    }

    public QueryBuilder Where(string condition)
    {
        commandSequence.Add(QBCommand.WHERE);
        columnSequence[commandIndex] = $" {condition} ";

        commandIndex++;


        return this;
    }

    public QueryBuilder And(string condition)
    {
        commandSequence.Add(QBCommand.AND);
        columnSequence[commandIndex] = $" {condition} ";

        commandIndex++;


        return this;
    }

    public QueryBuilder Or(string condition)
    {
        commandSequence.Add(QBCommand.OR);
        columnSequence[commandIndex] = $" {condition} ";

        commandIndex++;

        return this;
    }


    private bool ConnectionIsSet()
    {
        return this._connection != null;
    }

    [Obsolete("Eig. könnte ichs gleich rauslöschen. Aber der Trennungsschmerz")]
    private void AddParamsWithValue(Dictionary<string, string> paramMappings)
    {
        foreach (var mapping in paramMappings)
        {
        }
    }


    public QueryBuilder OrderBy(string column)
    {
        commandSequence.Add(QBCommand.ORDERBY);
        columnSequence[commandIndex] = $" {column} ";

        commandIndex++;

        return this;
    }

    public void Build()
    {
        string[] insertValueGroups;
        int paramIndex = 0;
        int insertIndex = 0;

        for (int i = 0; i < commandSequence.Count; i++)
        {
            var command = commandSequence[i];

            if (command == QBCommand.ADD_PARAM)
            {
                (int Index, string Key, dynamic Value) param = paramSequence[i];
                param.Key = param.Key[0] == '@' ? param.Key : '@' + param.Key;
            }
            else if (command == QBCommand.RAW_QUERY)
            {
                qString.Add(commandStringMappings[command]);
                qString.Add(columnSequence[i]);
            }
            else
            {
                qString.Add(commandStringMappings[command]);
            }
            if (columnSequence.TryGetValue(i, out var columns))
            {
                qString.Add(columns);
            }
        }

        queryString = qString.Aggregate((a, b) => a + b);

        calledBuild = true;
    }

    public IEnumerable<T> ReadMultiple<T>(ObjectBuilder<T> callback) where T : new()
    {
        // _connection.Open();

        using var command = new NpgsqlCommand(queryString, _connection);

        if (paramSequence.Count > 0)
        {
            foreach (var param in paramSequence)
            {
                var (_, Key, Value) = param.Value;

                command.Parameters.AddWithValue(Key, Value);
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
    // public IEnumerable<T> ReadMultiple<T>(ObjectBuilder<T> callback, NpgsqlCommand command) where T : new()
    // {
    //     // _connection.Open();

    //     if (paramMappings.Count > 0)
    //     {
    //         foreach (var mapping in paramMappings)
    //         {
    //             string key = mapping.Key[0] == '@' ? mapping.Key : '@' + mapping.Key;
    //             command.Parameters.AddWithValue(key, mapping.Value);
    //         }
    //     }

    //     using var reader = command.ExecuteReader();
    //     List<T> list = new();

    //     while (reader.Read())
    //     {
    //         T obj = new();
    //         callback(obj, reader);
    //         list.Add(obj);
    //     }

    //     return list;
    // }

    public QueryBuilder AddParam(string key, dynamic value)
    {
        commandSequence.Add(QBCommand.ADD_PARAM);
        var _key= key[0] == '@' ? key : '@' +key;
        paramSequence[commandIndex] = (commandIndex, _key, value);
        commandIndex++;

        return this;
    }

    public int ExecuteNonQuery(NpgsqlCommand command)
    {
        return command.ExecuteNonQuery();
    }


    // public ExecuteNonQuery()
    // {
    //     if (returnInsertedId)
    //         queryString += $" RETURNING id; ";

    //     using var command = new NpgsqlCommand(queryString, _connection);

    //     return command.ExecuteNonQuery();
    // }


    public void GetInsertedIds(bool returnsIds = true)
    {
        returnInsertedId = returnsIds;
    }

    protected void CloseConnection()
    {
        throw new NotImplementedException("");
    }

    public T? Run<T>(ObjectBuilder<T> builderDelegate) where T : class, new()
    {

        // if (queryString == string.Empty) throw new Exception("Invalid query provided.");

        // if (paramSequence.Count > 0)
        // {
        //     foreach (var param in paramSequence)
        //     {
        //         var (_, Key, Value) = param.Value;

        //         command.Parameters.AddWithValue(Key, Value);
        //     }
        // }

        using (var command = new NpgsqlCommand(queryString, _connection))
        {
            var resultArray = ReadMultiple<T>(builderDelegate).ToArray();
            return resultArray != null && resultArray.Length > 0 ? resultArray[0] : null;
        }
    }

    public QueryBuilder InsertAddValues(IEnumerable<string[]> rows)
    {
        insertedValues = true;

        if (rows == null || !rows.Any())
        {
            throw new ArgumentException("Rows collection cannot be null or empty.", nameof(rows));
        }

        string columns = Columns(rows.First());

        queryString += $" VALUES {string.Join(", ", rows.Select(row => $"({Columns(row)})"))}";

        return this;
    }

    public IEnumerable<T> Run<T>() where T : new()
    {
        if (queryString == string.Empty) throw new Exception("Invalid query provided.");

        using (var command = new NpgsqlCommand(queryString, _connection))
        {
            if (returnInsertedId)
            {
                if (!insertedValues) queryString += $" DEFAULT VALUES; ";

                return ReturnInsertedIds<T>(command);
            }
            else
            {
                throw new InvalidOperationException("Cannot call Run without a delegate when not returning inserted IDs.");
            }
        }
    }

    protected IEnumerable<T> ReturnInsertedIds<T>(NpgsqlCommand command) where T : new()
    {
        List<T> ids = new();

        using (NpgsqlDataReader reader = command.ExecuteReader())
        {
            if (queryString == string.Empty) throw new Exception("Invalid query provided.");

            if (paramSequence.Count > 0)
            {
                foreach (var param in paramSequence)
                {
                    var (_, Key, Value) = param.Value;

                    command.Parameters.AddWithValue(Key, Value);
                }
            }
            while (reader.Read())
            {
                T id = (T)reader[0];
                ids.Add(id);
            }
        }

        return ids;
    }


    // public int Run()
    // {
    //     if (queryString == string.Empty) throw new Exception("Invalid query provided.");

    //     using (var command = new NpgsqlCommand(queryString, _connection))
    //     {
    //         return ExecuteNonQuery();
    //     }
    // }



    public T? Read<T>(ObjectBuilder<T> callback) where T : class, new()
    {
        // _connection.Open();
        var res = ReadMultiple<T>(callback).ToList<T>();
        return (res != null && res.Count > 0) ? res[0] : null;

        using var command = new NpgsqlCommand(queryString, _connection);

        if (queryString == string.Empty) throw new Exception("Invalid query provided.");

        if (paramSequence.Count > 0)
        {
            foreach (var param in paramSequence)
            {
                var (_, Key, Value) = param.Value;

                command.Parameters.AddWithValue(Key, Value);
            }
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