using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FRS_AWSCSSync
{
    class Program
    {
        static void Main(string[] args)
        {
            ProcessMgr processMgr = new ProcessMgr();
            processMgr.Start();
            Console.ReadKey();
        }
    }
}
