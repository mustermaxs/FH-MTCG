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
    STRING_QUERY,
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
        { QBCommand.STRING_QUERY, "STRING_QUERY" },
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


    public void Reset()
    {
        qString = new();
        commandIndex = 0;
        commandSequence = new();
        columnSequence = new();
        paramSequence = new();
        queryString = string.Empty;
        paramMappings.Clear();
    }

    // public string Build()
    // {
    //     return queryString;
    // }

    public QueryBuilder Select(string[] columns)
    {
        commandSequence.Add(QBCommand.SELECT);
        columnSequence[commandIndex] = Columns(columns);
        commandIndex++;
        // queryString = queryString + $" SELECT {Columns(columns)} ";

        return this;
    }
    // public QueryBuilder Select(string query)
    // {
    //     queryString = queryString + $" SELECT {query} ";

    //     return this;
    // }

    public string Columns(string[] columns)
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



    public QueryBuilder Join(string table)
    {
        // queryString += $" JOIN {table}";
        commandSequence.Add(QBCommand.JOIN);
        columnSequence[commandIndex] = Columns(table.Split(","));
        commandIndex++;

        return this;
    }

    public QueryBuilder On(string joiningColumns)
    {
        // queryString += $" ON {joiningColumns}";
        commandSequence.Add(QBCommand.ON);
        columnSequence[commandIndex] = $" {joiningColumns} ";

        commandIndex++;

        return this;
    }

    public QueryBuilder From(string table)
    {
        // queryString += $" FROM {table}";
        commandSequence.Add(QBCommand.FROM);
        columnSequence[commandIndex] = $" {table} ";
        commandIndex++;

        return this;
    }

    public QueryBuilder Where(string condition)
    {
        // queryString += $" WHERE {condition}";
        commandSequence.Add(QBCommand.WHERE);
        columnSequence[commandIndex] = $" {condition} ";

        commandIndex++;


        return this;
    }

    public QueryBuilder And(string condition)
    {
        // queryString += $" AND {condition}";
        commandSequence.Add(QBCommand.AND);
        columnSequence[commandIndex] = $" {condition} ";

        commandIndex++;


        return this;
    }

    public QueryBuilder Or(string condition)
    {
        // queryString += $" OR {condition}";
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
            // comman
        }
    }


    public QueryBuilder OrderBy(string column)
    {
        // queryString += $" ORDER BY {column}";
        commandSequence.Add(QBCommand.ORDERBY);
        columnSequence[commandIndex] = $" {column} ";

        commandIndex++;

        return this;
    }

    public void Build()
    {
        // var cmd = new NpgsqlCommand(queryString, _connection);
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

                // if (param.Value is string)
                // {
                //     qString.Add($" {param.Key} = '{param.Value}' ");
                // }
                // else
                // {
                //     qString.Add($" {param.Key} = {param.Value} ");
                // }

            }
            //if insert statement, add the values from paramSequence
            // allows insertion of multiple values
            // e.g. INSERT INTO cards (id, name) VALUES(1,"goblin"), (3, "Knight")
            // if (command == QBCommand.VALUES)
            // {
                // insertIndex = insertIndex == 0 ? i : insertIndex;
                // int indexOfLast = paramSequence.Last(x => x.V)
                // bool isFirstValue = insertIndex == i;
                // string pre = isFirstValue ? "(" : "";
                // string post = 

                // qString.Add($" {pre} {paramSequence[i].Value}, {post}")

                // qString = 



                // continue;

            // }
            else
            {
                // if (command == )
                qString.Add(commandStringMappings[command]);
            }
            if (columnSequence.TryGetValue(i, out var columns))
            {
                qString.Add(columns);
            }

            // var cmdStringVal = commandStringMappings[command];
            // queryString += $" {cmdStringVal} ";
        }
        queryString = qString.Aggregate((a, b) => a + b);
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
        commandSequence.Add(QBCommand.ADD_PARAM);
        paramSequence[commandIndex] = (commandIndex, key, value);
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


    public void ShouldReturnInsertedId(bool returnsIds = true)
    {
        returnInsertedId = returnsIds;
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