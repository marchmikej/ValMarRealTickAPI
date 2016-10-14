using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ValMarRealTickAPI
{
    public static class Helper
    {
        public static void WriteLine(string fmt, params object[] args)
        {
            Console.WriteLine(fmt, args);
        }
    }
}
