using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdlinSoftware.Safe.Storage;
using LiteDB;

namespace EdlinSoftware.Safe.Services
{
    internal class LiteDbConnectionProvider : ILiteDbConnectionProvider
    {
        public LiteDbConnectionProvider(string fileName, string password)
        {

        }

        public ILiteDatabase GetDatabase()
        {
            throw new NotImplementedException();
        }
    }
}
