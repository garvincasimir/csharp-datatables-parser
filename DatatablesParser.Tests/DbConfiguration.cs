using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTablesParser.Tests
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.SqlServer;

    namespace MyNamespace
    {
        public class MyConfiguration : DbConfiguration
        {
            public MyConfiguration()
            {
                SetDefaultConnectionFactory(new LocalDbConnectionFactory("v11.0"));
            }
        }
    }
}
