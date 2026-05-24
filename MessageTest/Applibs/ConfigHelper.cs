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
        public static string ServiceUrl
        {
            get
                => $"http://*:8085";
        }

        /// <summary>
        ///     SQL連線字串
        /// </summary>
        public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["MessageTest"].ConnectionString;

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

        /// <summary>
        ///     RMQ帳號
        /// </summary>
        public static readonly string RabbitUserName = ConfigurationManager.AppSettings["RabbitUserName"];

        /// <summary>
        ///     RMQ密碼
        /// </summary>
        public static readonly string RabbitPassword = ConfigurationManager.AppSettings["RabbitPassword"];

        /// <summary>
        ///     RMQ URL
        /// </summary>
        public static readonly string RabbitMqUri = ConfigurationManager.AppSettings["RabbitMqUri"];

        /// <summary>
		///     本身Topic
		/// </summary>
		public static readonly string Topic = ConfigurationManager.AppSettings["Topic"];

        /// <summary>
		///     訂閱對象Topic
		/// </summary>
		public static readonly List<string> SubscribeTopics =
            ConfigurationManager.AppSettings["SubscribeTopics"].Split(',').ToList();
    }
}
