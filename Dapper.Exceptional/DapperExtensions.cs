using System;
using System.Collections.Generic;
using System.Data;

namespace Dapper.Exceptional
{
    public static class DapperExtensions
    {
        public static IEnumerable<T> DapperSql<T>(this IDbConnection db, string sql,
            object paramList = null, IDbTransaction trans = null, int? timeoutSeconds = null)
        {
            try
            {
                return db.Query<T>(sql, paramList, trans, commandTimeout: timeoutSeconds);
            }
            catch (Exception orig)
            {
                var ex = new Exception("Dapper sql execution failed!", orig);
                AddSqlDetailsToException(ex, sql, paramList);
                throw ex;
            }
        }

        public static IEnumerable<T> DapperProc<T>(this IDbConnection db, string procName,
            object paramList = null, IDbTransaction trans = null, int? timeoutSeconds = null)
        {
            try
            {
                return db.Query<T>(procName, paramList, trans, 
                    commandTimeout:timeoutSeconds, commandType:CommandType.StoredProcedure);
            }
            catch (Exception orig)
            {
                var ex = new Exception("Dapper proc execution failed!", orig);
                AddProcDetailsToException(ex, procName, paramList);
                throw ex;
            }
        }

        public static int DapperProcNonQuery(this IDbConnection db, string procName,
            object paramList = null, IDbTransaction trans = null, int? timeoutSeconds = null)
        {
            try
            {
                return db.Execute(procName, paramList, trans, timeoutSeconds, CommandType.StoredProcedure);
            }
            catch (Exception orig)
            {
                var ex = new Exception("Dapper proc execution failed!", orig);
                AddProcDetailsToException(ex, procName, paramList);
                throw ex;
            }
        }

        private static void AddSqlDetailsToException(Exception ex, string sql, object paramList)
        {
            ex.Data.Add("InlineSql", sql);
            AddParametersToException(ex, paramList);
        }

        private static void AddProcDetailsToException(Exception ex, string procName, object paramList)
        {
            ex.Data.Add("Procedure", procName);
            AddParametersToException(ex, paramList);
        }

        private static void AddParametersToException(Exception ex, object paramList)
        {
            if (paramList is DynamicParameters dynParams) //C# 7 syntax
            {
                foreach (var p in dynParams.ParameterNames)
                {
                    ex.Data.Add(p, dynParams.Get<object>(p).ToString());
                }
            }
            else
            {
                var props = paramList.GetType().GetProperties();
                foreach (var prop in props)
                {
                    ex.Data.Add(prop.Name, prop.GetValue(paramList).ToString());
                }
            }
        }
    }
}
