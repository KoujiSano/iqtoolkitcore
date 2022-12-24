using IQToolkit.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace IQToolkitCore.Data.Sqlite
{
    internal static class SQLiteDbTypeHelper
    {
        public static SqliteType ToSqliteType(this SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.BigInt:
                    return SqliteType.Integer;
                case SqlType.Binary:
                    return SqliteType.Integer;
                case SqlType.Bit:
                    return SqliteType.Integer;
                case SqlType.Char:
                    return SqliteType.Text;
                case SqlType.Date:
                    return SqliteType.Text;
                case SqlType.DateTime:
                case SqlType.SmallDateTime:
                    return SqliteType.Text;
                case SqlType.DateTime2:
                    return SqliteType.Text;
                case SqlType.DateTimeOffset:
                    return SqliteType.Text;
                case SqlType.Decimal:
                    return SqliteType.Text;
                case SqlType.Float:
                case SqlType.Real:
                    return SqliteType.Real;
                case SqlType.Image:
                    return SqliteType.Blob;
                case SqlType.Int:
                    return SqliteType.Integer;
                case SqlType.Money:
                case SqlType.SmallMoney:
                    return SqliteType.Text;
                case SqlType.NChar:
                    return SqliteType.Text;
                case SqlType.NText:
                case SqlType.NVarChar:
                    return SqliteType.Text;
                case SqlType.SmallInt:
                    return SqliteType.Integer;
                case SqlType.Text:
                    return SqliteType.Text;
                case SqlType.Time:
                    return SqliteType.Text;
                case SqlType.Timestamp:
                    return SqliteType.Text;
                case SqlType.TinyInt:
                    return SqliteType.Integer;
                case SqlType.Udt:
                    return SqliteType.Text;
                case SqlType.UniqueIdentifier:
                    return SqliteType.Text;
                case SqlType.VarBinary:
                    return SqliteType.Blob;
                case SqlType.VarChar:
                    return SqliteType.Text;
                case SqlType.Variant:
                    return SqliteType.Text;
                case SqlType.Xml:
                    return SqliteType.Text;
                default:
                    throw new InvalidOperationException(string.Format("Unhandled sql type: {0}", sqlType));
            }
        }
    }
}
