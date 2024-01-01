// using System;
// using System.Collections.Generic;
// using System.Data;
// using Npgsql;

// namespace MTCG;
// // public delegate void FillMethod<T>(T obj, IDataReader reader);

// public enum QBCommand
// {
//     SELECT,
//     INSERT,
//     WHERE,
//     ON,
//     UPDATE,
//     SET,
//     JOIN,
//     FROM,
//     RAW_QUERY,
//     AND,
//     OR,
//     IN,
//     ADD_PARAM,
//     ORDERBY,
//     INSERT_VALUES,
//     VALUES_DECL,
//     ADD_INSERT_VALUE,
//     INSERT_DEFAULT,
//     BLANK,
//     VALUES_DEF
// }

// public abstract class BaseQuery
// {
//     public QBCommand Command { get; set; }
//     public List<string> Columns { get; set; } = new List<string>();
//     public abstract string BuildFragment();
//     public bool CommandFinished { get; set; } = false;

// }

// public class Token
// {
//     public QBCommand Command { get; set; }
//     public List<string> Columns { get; set; } = new List<string>();
//     public void AddColumn(string name)
//     {
//         Columns.Add(name);
//     }

//     public void
// }


// public class BetterQuery
// {
//     protected List<QBCommand>
// }