using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Adlg2Helper;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            while (true)
            {
                var adlClient = Adlg2ClientFactory.BuildPathClient("domodatalakeint",
                    "2iFodYH0ZDItjkpVdoRp+Y3OjEUdWhT1REg80xw4TYmLQwX3tY+DLy0m2vASCoCq1tD413ytVSSUsU0ljunxCA==");
                var sw = new Stopwatch();
                sw.Start();
                var directories = adlClient.List("messagebridge", true,
                    "MessageBridge.ProofOfConcept.Sales.Invoice/21090", timeout: null).ToList();
                sw.Stop();
                Console.WriteLine($"Fetched {directories.Count} in {sw.Elapsed}");
                Thread.Sleep(1000);
            }
        }
    }
}
