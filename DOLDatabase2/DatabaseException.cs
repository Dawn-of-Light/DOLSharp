using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOL.Database2
{
    class DatabaseException :Exception
    {
        public DatabaseException(string Message)
            : base(Message)
        {
        }
        public DatabaseException() : this("")
        {
        }
    }
    class CouldNotConnectException : DatabaseException
    {
        public CouldNotConnectException(string Message) :
            base("Could not connect: " + Message)
        {
        }
        public CouldNotConnectException():
            this("unknown reason")
        {
        }
    }
    class NotConnectedException : DatabaseException
    {
        public NotConnectedException (string Message) :
            base("DatabaseProvider not connected" + Message)
        {
        }
        public NotConnectedException() : this("")
        {
        }
  
    }
}
