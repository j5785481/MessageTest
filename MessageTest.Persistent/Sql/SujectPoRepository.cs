using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using MessageTest.Domain.DTO;
using MessageTest.Domain.Model;
using MessageTest.Domain.Repository;

namespace MessageTest.Persistent.Sql
{
    public class SujectPoRepository : ISubjectRepository
    {
        /// <summary>
        /// 連線字串
        /// </summary>
        private string connectionString;

        public SujectPoRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public (Exception exception, Subject subject) Add(AddSubjectRequestDto req)
        {
            try
            {
                using (var cn = new SqlConnection(this.connectionString))
                {
                    var result = cn.QueryFirstOrDefault<SubjectPo>(
                        "pro_sujectAdd",
                        new
                        {
                            f_title = req.SubjectTitle
                        },
                        commandType: CommandType.StoredProcedure);

                    return (null, result);
                }
            }
            catch (Exception ex)
            {
                return (ex, null);
            }
        }
    }
}
