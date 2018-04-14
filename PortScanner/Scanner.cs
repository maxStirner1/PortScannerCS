using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace PortScanner
{

    public partial class Program
    {
        static void StartScanner(List<IPAddress> listOfIpAddresses, string nrOfH, string nrOfT)
        {
            int nrOfHosts = Int32.Parse(nrOfH);
            int nrOfThreads = Int32.Parse(nrOfT);
            const int nrOfPorts = 65535;
            List<IPAddress> list = listOfIpAddresses;

            List<Thread> hostThreads = new List<Thread>();
            
            for (int i = 1; i <= nrOfHosts; i++)
            {
                int thread_i = i;
                hostThreads[i] = new Thread(() => HostThreadWorker(list, nrOfHosts, nrOfThreads, nrOfPorts, thread_i));
                hostThreads[i].Start();
            }
        }

        private static void HostThreadWorker(List<IPAddress> list, int nrOfHosts, int nrOfThreads, int nrOfPorts, int i)
        {
            int totalNrOfHosts = list.Count;
            int counter = 0;
            IPAddress ipAddrScanned = new IPAddress(0);

            object o = counter;
            List<Thread> portThreads = new List<Thread>();
            List<CountdownEvent> countdownEvents = new List<CountdownEvent>();
            countdownEvents[i] = new CountdownEvent(nrOfPorts);



            Monitor.Enter(o);

            while (counter <= totalNrOfHosts)
            {
                {
                    ipAddrScanned = list[i];
                    Monitor.Enter(o);
                    counter++;
                    Monitor.Exit(o);
                }
                
               



                for (int j = 0; j < nrOfThreads; j++)
                {
                    portThreads[j] =
                        new Thread(() => PortThreadWorker(ipAddrScanned, nrOfPorts, j, countdownEvents[i]));
                    portThreads[j].Start();
                }

                countdownEvents[i].Wait();

            }
        }

        private static void PortThreadWorker(IPAddress ipAddrScannned, int nrOfPorts, int j, CountdownEvent countdownEvent)
        {
            List<PortList> portLists = new List<PortList>();

            portLists[j] = new PortList();

            int[] port = new int[j];
            for (int i = 0; i < j; i++)
            {
                port[i] = 1;
            }

            List<TcpClient> tcpClients = new List<TcpClient>();
            tcpClients[j] = new TcpClient();
            
            while (port[j] != -1)
            {
                Monitor.Enter(portLists[j]);
                port[j] = portLists[j].getNext();
                Monitor.Exit(portLists[j]);
                try
                {
                    tcpClients[j] = new TcpClient(ipAddrScannned.ToString(), port[j]);
                }
                catch
                {
                    continue;
                }
                finally
                {
                    try
                    {
                        tcpClients[j].Close();
                    }
                    catch
                    {
                    }
                }
                
                Console.WriteLine("TCP Port " + port[j] + " is open");
            }

            countdownEvent.Signal();
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
            public PortList() : this(1, 65535)
            {
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
    }
}