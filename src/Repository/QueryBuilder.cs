using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace MTCG;
// public delegate void FillMethod<T>(T obj, IDataReader reader);


// OBSOLETE voll unnötig, wieso hab ich das nochmal gemacht?
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
    VALUES_DECL,
    ADD_INSERT_VALUE,
    INSERT_DEFAULT,
    BLANK,
    VALUES_DEF,
    DELETE,
    LIMIT,
    RETURNING
}

public delegate void ObjectBuilder<T>(T obj, IDataReader reader);
public class ValueTypeWrapper<T>
{
    public T? Value { get; set; }

    public ValueTypeWrapper()
    {
        this.Value = default(T);
    }
}

// TODO write tests
// TODO methoden vereinfachen und abstrahieren

/// <summary>
/// fix this ugly thing.
/// </summary>
public class QueryBuilder : IDisposable
{
    private string queryString = string.Empty;
    protected int paramIndex = 0;
    protected bool returnInsertedId;
    protected bool insertedValues = false;
    private int expectNbrOfValuesInserted = 0;
    protected bool isInsertStatement = false;
    private NpgsqlConnection? _connection = null;
    protected bool calledBuild = false;
    private Dictionary<string, dynamic> paramMappings = new Dictionary<string, dynamic>();
    protected List<QBCommand> commandSequence = new();
    protected Dictionary<int, (int Index, string Key, dynamic Value)> paramSequence = new();
    protected Dictionary<int, string> columnSequence = new();
    protected int commandIndex = 0;
    protected bool isFirstUpdateSetStmt = true;
    protected int qsIndex = 0;
    protected List<string> qString = new();
    protected Dictionary<QBCommand, string> commandStringMappings = new()
    {
        { QBCommand.SELECT, " SELECT" },
        { QBCommand.INSERT, " INSERT INTO " },
        { QBCommand.INSERT_DEFAULT, " INSERT INTO " },
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
        { QBCommand.VALUES_DEF, " " },
        { QBCommand.VALUES_DECL, " VALUES " },
        {QBCommand.BLANK, "  "},
        {QBCommand.DELETE, " DELETE "},
        {QBCommand.LIMIT, " LIMIT "},
        {QBCommand.RETURNING, " RETURNING "}
    };
    private bool isFirstInsertValuesCall = true;
    private bool insertDefaultValues = false;

    public QueryBuilder(NpgsqlConnection? connection)
    {
        if (connection == null) throw new ArgumentNullException(nameof(connection));

        this._connection = connection;
    }

    public void Dispose()
    {

        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
            // Console.WriteLine("CLOSED CONNECTION");
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                Console.WriteLine("!!!!!!!!!!!!!!!!Connection is open.");
            }
        }
    }

    ~QueryBuilder()
    {
        Dispose();
    }

    public QueryBuilder Limit(string condition)
    {
        commandSequence.Add(QBCommand.LIMIT);
        columnSequence[commandIndex] = $" {condition} ";

        commandIndex++;

        return this;
    }

    public QueryBuilder InsertInto(string tableName, IEnumerable<string>? fields)
    {
        expectNbrOfValuesInserted = fields?.Count() ?? 0;
        isInsertStatement = true;
        commandSequence.Add(QBCommand.INSERT);
        columnSequence[commandIndex] = fields != null && fields.Count() > 0
            ? $" {tableName} ({Columns(fields)}) "
            : $" {tableName} ";
        commandIndex++;

        return this;
    }
    public QueryBuilder InsertInto(string tableName, params string[]? fields)
    {
        expectNbrOfValuesInserted = fields?.Count() ?? 0;
        isInsertStatement = true;
        commandSequence.Add(QBCommand.INSERT);

        columnSequence[commandIndex] = fields != null && fields.Count() > 0
            ? $" {tableName} ({Columns(fields)}) "
            : $" {tableName} ";
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
        insertDefaultValues = false;
        qString = new();
        commandIndex = 0;
        commandSequence = new();
        columnSequence = new();
        paramSequence = new();
        queryString = string.Empty;
        paramMappings.Clear();
        paramIndex = 0;
        qsIndex = 0;
        isFirstInsertValuesCall = true;
        isFirstUpdateSetStmt = true;
        insertedValues = false;
        expectNbrOfValuesInserted = 0;
        isInsertStatement = false;
        returnInsertedId = false;
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

    public QueryBuilder InsertValues(params string[]? columns)
    {
        if (columns == null || columns.Length == 0)
        {
            insertDefaultValues = true;

            return this;
        }

        int columnCount = columns?.Length ?? 0;
        if (columnCount < expectNbrOfValuesInserted) throw new ArgumentException($"Expected {expectNbrOfValuesInserted} values to be inserted.");
        if (columns != null && columns.Length > 0)
            commandSequence.Add(QBCommand.VALUES_DEF);
        else
            commandSequence.Add(QBCommand.BLANK);


        if (isFirstInsertValuesCall)
        {
            columnSequence[commandIndex] = columns != null && columns.Length > 0 ? $"VALUES ({Columns(columns)}) " : "";
        }

        else
        {
            if (commandIndex > 0 && commandSequence[commandIndex - 1] == QBCommand.VALUES_DEF)
            {
                columnSequence[commandIndex] = columns != null && columns.Length > 0 ? $" ,({Columns(columns)}) " : "";
            }

            else
            {
                columnSequence[commandIndex] = columns != null && columns.Length > 0 ? $" ({Columns(columns)}) " : "";
            }
        }

        isFirstInsertValuesCall = false;
        commandIndex++;

        return this;
    }
    public QueryBuilder InsertValues(IEnumerable<string>? columns)
    {
        int columnCount = columns?.Count() ?? 0;
        if (columnCount < expectNbrOfValuesInserted) throw new ArgumentException($"Expected {expectNbrOfValuesInserted} values to be inserted.");
        if (columns != null && columns.Count() > 0)
            commandSequence.Add(QBCommand.VALUES_DEF);
        else
            commandSequence.Add(QBCommand.BLANK);

        columnSequence[commandIndex] = columns != null && columns.Count() > 0 ? $" ({Columns(columns)}) " : "";
        commandIndex++;

        return this;
    }


    public QueryBuilder Update(string tableName)
    {
        commandSequence.Add(QBCommand.UPDATE);
        columnSequence[commandIndex] = $" {tableName} ";

        commandIndex++;

        return this;
    }

    public QueryBuilder UpdateSet(string column, string value)
    {
        string pre = isFirstUpdateSetStmt ? " SET " : "";
        string commaOrNothing = isFirstUpdateSetStmt ? "" : " , ";

        commandSequence.Add(QBCommand.BLANK);
        columnSequence[commandIndex] = $" {pre} {commaOrNothing} {column}={value} ";

        isFirstUpdateSetStmt = false;
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

            if (command == QBCommand.RAW_QUERY)
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
        // Console.WriteLine(queryString);
        calledBuild = true;
    }

    protected void AddParams(NpgsqlCommand command)
    {
        if (paramSequence.Count > 0)
        {
            foreach (var param in paramSequence)
            {
                var (_, Key, Value) = param.Value;

                command.Parameters.AddWithValue(Key, Value);
            }
        }
    }

    public IEnumerable<T> ReadMultiple<T>(ObjectBuilder<T> callback) where T : new()
    {
        if (!calledBuild) throw new Exception("Need to call QueryBuilder.Build first.");

        using var command = new NpgsqlCommand(queryString, _connection);

        AddParams(command);

        using var reader = command.ExecuteReader();
        List<T> list = new();

        while (reader.Read())
        {
            T obj = new();
            callback(obj, reader);
            list.Add(obj);
        }
        command.Dispose();
        reader.DisposeAsync();
        return list;
    }

    public QueryBuilder AddParam(string key, dynamic value)
    {
        // commandSequence.Add(QBCommand.ADD_PARAM);
        var _key = key[0] == '@' ? key : '@' + key;
        paramSequence[paramIndex++] = (commandIndex, _key, value);
        // commandIndex++;

        return this;
    }

    public int ExecuteNonQuery()
    {
        using (var command = new NpgsqlCommand(queryString, _connection))
        {
            AddParams(command);

            if (returnInsertedId)
                queryString += " RETURN id";

            return command.ExecuteNonQuery();
        }
    }


    public QueryBuilder GetInsertedIds(bool returnsIds = true)
    {
        returnInsertedId = returnsIds;

        return this;
    }

    protected void CloseConnection()
    {
        throw new NotImplementedException("");
    }

    public T? Run<T>(ObjectBuilder<T> builderDelegate) where T : class, new()
    {
        using (var command = new NpgsqlCommand(queryString, _connection))
        {
            var resultArray = ReadMultiple<T>(builderDelegate).ToArray();
            return resultArray != null && resultArray.Length > 0 ? resultArray[0] : null;
        }
    }

    public QueryBuilder InsertAddValues(IEnumerable<string[]> rows)
    {
        if (insertDefaultValues) throw new Exception("Can't insert values while applying DEFAUL VALUES command");

        if (rows == null || !rows.Any())
        {
            throw new ArgumentException("Rows collection cannot be null or empty.", nameof(rows));
        }

        string columns = Columns(rows.First());

        queryString += $" VALUES {string.Join(", ", rows.Select(row => $"({Columns(row)})"))}";

        return this;
    }

    public QueryBuilder InsertOnlyDefaultValues()
    {
        this.insertDefaultValues = true;

        return this;
    }


    public QueryBuilder DeleteFrom(string tableName)
    {
        commandSequence.Add(QBCommand.DELETE);
        commandIndex++;

        return From(tableName);
    }

    public IEnumerable<T> Run<T>() where T : new()
    {
        if (queryString == string.Empty) throw new Exception("Invalid query provided.");

        if (returnInsertedId)
        {
            if (insertDefaultValues) queryString += $" DEFAULT VALUES; ";
            using var command = new NpgsqlCommand(queryString, _connection);
            return ReturnInsertedIds<T>(command);
        }
        else
        {
            throw new InvalidOperationException("Cannot call Run without a delegate when not returning inserted IDs.");
        }
    }



    protected IEnumerable<T> ReturnInsertedIds<T>(NpgsqlCommand command) where T : new()
    {
        List<T> ids = new();

        using (var reader = command.ExecuteReader())
        {
            if (queryString == string.Empty) throw new Exception("Invalid query provided.");

            AddParams(command);

            while (reader.Read())
            {
                T id = (T)reader[0];
                ids.Add(id);
            }
        }
        command.Dispose();

        return ids;
    }

    public IEnumerable<Dictionary<string, object>> ReadMultiple()
    {
        if (!calledBuild) throw new Exception("Need to call QueryBuilder.Build first.");

        using var command = new NpgsqlCommand(queryString, _connection);

        AddParams(command);

        using var reader = command.ExecuteReader();
        List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();

        while (reader.Read())
        {
            var recordValues = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                object value = reader.GetValue(i);
                recordValues[columnName] = value;
            }

            resultList.Add(recordValues);
        }

        command.Dispose();
        reader.DisposeAsync();
        return resultList;
    }


    public QueryBuilder Returning(string columnName)
    {
        commandSequence.Add(QBCommand.RETURNING);
        columnSequence[commandIndex] = $" {columnName} ";

        commandIndex++;

        return this;
    }


    /// <summary>
    /// Reads a single value from the specified column and attempts
    /// to cast to the provided type.
    /// </summary>
    /// <typeparam name="T">Expected return type.</typeparam>
    /// <param name="columnName">Name of DB column.</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T? ReadSingle<T>(string columnName) where T : struct
    {
        if (!calledBuild) throw new Exception("Need to call QueryBuilder.Build first.");

        if (insertDefaultValues) queryString += $" DEFAULT VALUES ";
        if (isInsertStatement) queryString += $" RETURNING {columnName} ";

        // Console.WriteLine(queryString);
        var result = ReadMultiple().FirstOrDefault();

        if (result == null || !result.ContainsKey(columnName))
            return default(T);

        return (T)result[columnName];
    }

    public T? Read<T>(ObjectBuilder<T> callback) where T : class, new()
    {
        if (!calledBuild) throw new Exception("Need to call QueryBuilder.Build first.");

        var res = ReadMultiple<T>(callback).ToList<T>();

        return (res != null && res.Count > 0) ? res[0] : null;

    }
}