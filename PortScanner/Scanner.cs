using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace PortScanner
{
    public partial class Program
    {
        static void StartScanner(List<IPAddress> listOfIpAddresses, string nrOfH, string nrOfT)
        {
            int nrOfHosts = Int32.Parse(nrOfH);
            int nrOfThreads = Int32.Parse(nrOfT);
            // const int nrOfPorts = 100;
            List<IPAddress> list = listOfIpAddresses;
            if (nrOfHosts > list.Count) nrOfHosts = list.Count;
            MutexClass mutobj = new MutexClass();
            mutobj._mutex = new Mutex();

            // List<Thread> hostThreads = new List<Thread>();

            for (int i = 0; i < nrOfHosts; i++)
            {
                int thread_i = i;
                List<IPAddress> l = list;
                int noh = nrOfHosts;
                int not = nrOfThreads;
                MutexClass m = mutobj;
                //int nop = nrOfPorts;
                var hostThread = new Thread(() => HostThreadWorker(l, noh, not, thread_i,  m));
                hostThread.Start();
            }
        }


        private static void HostThreadWorker(List<IPAddress> list, int nrOfHosts, int nrOfThreads,
            int i, MutexClass m)
        {
            int totalNrOfHosts = list.Count;
            int counter = 0;
            IPAddress ipAddrScanned = new IPAddress(0);

            //object o = counter;
            //List<Thread> portThreads = new List<Thread>();
            //List<CountdownEvent> countdownEvents = new List<CountdownEvent>();
            // CountdownEvent cdeEvent = new CountdownEvent(nrOfPorts);
            
            PortList portListing = new PortList(1, 600);
           Cde cdeobj = new Cde();
            cdeobj._countdownEvent = new CountdownEvent(nrOfThreads);
//            List<CountdownEvent> countdownEvents = new List<CountdownEvent>();
//            for (int j = 0; j < nrOfHosts; j++)
//            {
//                countdownEvents[j] = new CountdownEvent(nrOfThreads);
//            }
            
            while ((i + counter) <= (totalNrOfHosts - 1))
            {
                {
                    ipAddrScanned = list[i + counter];
                    portListing = new PortList(1, 600);
                     cdeobj = new Cde();
                    cdeobj._countdownEvent = new CountdownEvent(nrOfThreads);

                    m._mutex.WaitOne();
                   
                    try
                    {
                        counter += nrOfHosts;
                    }

                    finally
                    {
                        m._mutex.ReleaseMutex();
                    }
                }

                 string str = string.Concat("Machine ", ipAddrScanned, " is being scanned");
                Console.WriteLine(str);
                str.WriteDebug();

                for (int j = 0; j < nrOfThreads; j++)
                {
                    int k = j;
                    IPAddress ip = ipAddrScanned;
                    // CountdownEvent cde = cdeEvent;
                    //int nop = nrOfPorts;
                    PortList p = portListing;
                    //Cde cdevent = cdeobj;
                    Cde cdevent = cdeobj;
                    var portThread =
                        new Thread(() => PortThreadWorker(ip, k, cdevent, p));
                    portThread.Start();
                }

                cdeobj._countdownEvent.Wait();
                
            }
        }

        private static void PortThreadWorker(IPAddress ipAddrScannned, int j,
            Cde cdeevent, PortList portListing)
        {
            //List<PortList> portLists = new List<PortList>();

            // PortList portListing = new PortList();

//            int[] port = new int[j];
//            for (int i = 0; i < j; i++)
//            {
//                port[i] = 1;
//            }

            //List<TcpClient> tcpClients = new List<TcpClient>();
            TcpClient tcpClientobj = new TcpClient();
            UdpClient udpClientobj = new UdpClient();
            Mutex mut = new Mutex();
            int port = 1;
            //object o = portListing;
            while (port != -1)
            {
                mut.WaitOne();
                try
                {
                    port = portListing.getNext();
                }

                finally
                {
                    mut.ReleaseMutex();
                }

                try
                {
                    tcpClientobj = new TcpClient(ipAddrScannned.ToString(), port);
                    udpClientobj = new UdpClient(ipAddrScannned.ToString(), port);
                }
                catch
                {
                    continue;
                }
                finally
                {
                    try
                    {
                        tcpClientobj.Close();
                        udpClientobj.Close();
                    }
                    catch
                    {
                    }
                }

                string s = string.Concat("Port " , port , " is open on machine " , ipAddrScannned);
                Console.WriteLine(s);
                s.WriteDebug();
            }

            cdeevent._countdownEvent.Signal();
        }

        public class PortList
        {
            private int start;
            private int stop;
            private int ptr;

            public PortList(int start, int stop)
            {
                this.start = start;
                this.stop = stop;
                this.ptr = start;
            }

            public bool hasMore()
            {
                return (stop - ptr) >= 0;
            }

            public int getNext()
            {
                if (hasMore())
                    return ptr++;
                return -1;
            }
        }

        public class Cde
        {
            public CountdownEvent _countdownEvent;
        }

        public class MutexClass
        {
            public Mutex _mutex;
        }
    }

    public static class LoggingExtensions
    {
        static ReaderWriterLock locker = new ReaderWriterLock();
        private static bool _filenameExists;
        static string filename = String.Empty;
        public static void WriteDebug(this string text)
        {
            try
            {
                locker.AcquireWriterLock(int.MaxValue);
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);

                Directory.CreateDirectory(Path.GetDirectoryName(path) + "/Logs");
                

                
                if (_filenameExists == false)
                {
                    filename = string.Concat("Portscanner_", DateTime.Now.Ticks.ToString(), ".txt");
                    _filenameExists = true;
                }

                // Path.GetDirectoryName(path);
                System.IO.File.AppendAllLines(
                    Path.Combine(
                        Path.GetDirectoryName(path), "Logs"
                        , filename), new[] {text});
            }
            finally
            {
                locker.ReleaseWriterLock();
            }
        }
    }
}