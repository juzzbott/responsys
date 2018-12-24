using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Responsys.UI.Services
{
    public class BaseLiteDbService
    {

        private const string ConnectionStringKey = "LiteDB";

        protected string ConnectionString { get; set; }

        public BaseLiteDbService()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings[ConnectionStringKey].ConnectionString;
        }

    }
}
