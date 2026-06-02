using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageTest.Tests.Applibs
{
    public class ConfigHelper
    {
        public static string MongoConn = @"mongodb://localhost:27017";

        public static string AffixKey = "MessageTest";
        public static string MongoDataBaseName = "MessageTest";
        public static string RedisConn { get; set; } = @"localhost:16379";

        public static int DataBase { get; set; } = 0;
    }
}
