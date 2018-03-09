using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThinkEco
{
    public class Exception_STOP : System.Exception
    {
        public Exception_STOP()
        {
        }

        public Exception_STOP(string message) : base(message)
        {
        }
    }

    public class Exception_FAIL : System.Exception
    {
        public Exception_FAIL()
        {
        }

        public Exception_FAIL(string message) : base(message)
        {
        }
    }
}
