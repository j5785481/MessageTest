using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Applibs
{
    internal class ConfigHelper
    {
        /// <summary>
		///     Redis連線字串
		/// </summary>
		public static readonly string RedisConn = ConfigurationManager.ConnectionStrings["Redis"].ConnectionString;

        /// <summary>
		///     Redis前贅詞
		/// </summary>
		public static readonly string RedisAffixKey = ConfigurationManager.AppSettings["RedisAffixKey"];

        /// <summary>
        ///     REDIS DB
        /// </summary>
        public static readonly int RedisDataBase = Convert.ToInt32(ConfigurationManager.AppSettings["RedisDataBase"]);

        /// <summary>
		///     芒果連線字串
		/// </summary>
		public static string MongoDBConn = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
    }
}
