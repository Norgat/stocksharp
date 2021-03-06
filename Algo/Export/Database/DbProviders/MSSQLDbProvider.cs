﻿namespace StockSharp.Algo.Export.Database.DbProviders
{
	using System;
	using System.Collections.Generic;
	using System.Data.SqlClient;
	using System.Data;
	using System.Linq;
	using System.Text;

	using Ecng.Common;
	using Ecng.Xaml.Database;

	internal class MSSQLDbProvider : BaseDbProvider
	{
		public MSSQLDbProvider(DatabaseConnectionPair connection)
			: base(connection)
		{
		}

		public override void InsertBatch(Table table, IEnumerable<IDictionary<string, object>> parameters)
		{
			if (CheckUnique)
			{
				using (var connection = Database.CreateConnection())
				{
					foreach (var value in parameters)
					{
						using (var command = CreateCommand(connection, CreateInsertSqlString(table, value), value.ToDictionary(par => "@" + par.Key, par => par.Value)))
							command.ExecuteNonQuery();
					}
				}

				return;
			}

			using (var con = Database.CreateConnection())
			using (var blkcopy = new SqlBulkCopy((SqlConnection)con, SqlBulkCopyOptions.KeepIdentity, null))
			{
				blkcopy.DestinationTableName = table.Name;
				blkcopy.WriteToServer(CreateTable(table, parameters));
			}
		}

		private static string CreateInsertSqlString(Table table, IDictionary<string, object> parameters)
		{
			var sb = new StringBuilder();
			sb.AppendLine("IF NOT EXISTS (SELECT * FROM {0} WHERE {1})".Put(table.Name, table.Columns.Where(c => c.IsPrimaryKey).Select(c => "{0} = @{0}".Put(c.Name)).Join(" AND ")));
			sb.AppendLine("BEGIN");
			sb.Append("INSERT INTO ");
			sb.Append(table.Name);
			sb.Append(" (");
			foreach (var par in parameters)
			{
				sb.Append("[");
				sb.Append(par.Key);
				sb.Append("],");
			}
			sb.Remove(sb.Length - 1, 1);
			sb.Append(") VALUES (");
			foreach (var par in parameters)
			{
				sb.Append("@");
				sb.Append(par.Key);
				sb.Append(",");
			}
			sb.Remove(sb.Length - 1, 1);
			sb.AppendLine(")");
			sb.Append("END");
			return sb.ToString();
		}

		private static DataTable CreateTable(Table table, IEnumerable<IDictionary<string, object>> parameters)
		{
			var result = new DataTable(table.Name);

			var primaryKeys = new List<DataColumn>();

			foreach (var column in table.Columns)
			{
				var dbType = column.DbType;

				if (dbType.IsGenericType && dbType.IsNullable())
					dbType = dbType.GetUnderlyingType();

				var dbCol = result.Columns.Add(column.Name, dbType);

				if (column.IsPrimaryKey)
					primaryKeys.Add(dbCol);
			}

			result.PrimaryKey = primaryKeys.ToArray();

			foreach (var param in parameters)
			{
				var row = result.NewRow();

				foreach (var pair in param)
					row[pair.Key] = pair.Value ?? DBNull.Value;

				result.Rows.Add(row);
			}

			return result;
		}

		protected override string CreatePrimaryKeyString(IEnumerable<ColumnDescription> columns)
		{
			var str = columns.Select(c => "[{0}]".Put(c.Name)).Join(",");

			if (str.IsEmpty())
				return null;

			return "PRIMARY KEY (" + str + ")";
		}

		protected override string GetDbType(Type t, object restriction)
		{
			if (t == typeof(DateTimeOffset))
				return "DATETIMEOFFSET";
			if (t == typeof(DateTime))
				return "DATETIME2";
			if (t == typeof(TimeSpan))
				return "TIME";
			if (t == typeof(Guid))
				return "GUID";
			if (t == typeof(bool))
				return "bit";

			return base.GetDbType(t, restriction);
		}
	}
}