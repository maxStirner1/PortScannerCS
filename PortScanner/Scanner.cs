﻿using System;
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
           // const int nrOfPorts = 100;
            List<IPAddress> list = listOfIpAddresses;
            if (nrOfHosts > list.Count) nrOfHosts = list.Count;

           // List<Thread> hostThreads = new List<Thread>();
            
            for (int i = 0; i < nrOfHosts; i++)
            {
                int thread_i = i;
                List<IPAddress> l = list;
                int noh = nrOfHosts;
                int not = nrOfThreads;
                //int nop = nrOfPorts;
                var hostThread = new Thread(() => HostThreadWorker(l, noh, not, thread_i));
                hostThread.Start();
            }
        }
        
       

        private static void HostThreadWorker(List<IPAddress> list, int nrOfHosts, int nrOfThreads, 
             int i)
        {
            int totalNrOfHosts = list.Count;
            int counter = 0;
            IPAddress ipAddrScanned = new IPAddress(0);

            //object o = counter;
            //List<Thread> portThreads = new List<Thread>();
            //List<CountdownEvent> countdownEvents = new List<CountdownEvent>();
           // CountdownEvent cdeEvent = new CountdownEvent(nrOfPorts);
            Mutex mut = new Mutex();
            PortList portListing = new PortList(1,600);
            Cde cdeobj = new Cde();
            cdeobj._countdownEvent = new CountdownEvent(nrOfThreads);


           

            while ((i+counter) < totalNrOfHosts)
            {
                {
                    
                    ipAddrScanned = list[i+counter];
                    mut.WaitOne();
                    try
                    {
                        counter+=nrOfHosts;
                    }

                    finally
                    {
                        mut.ReleaseMutex();
                    }
                }
                
               



                for (int j = 0; j < nrOfThreads; j++)
                {
                    int k = j;
                    IPAddress ip = ipAddrScanned;
                   // CountdownEvent cde = cdeEvent;
                    //int nop = nrOfPorts;
                    PortList p = portListing;
                    Cde cdevent = cdeobj;
                    var portThread =
                        new Thread(() => PortThreadWorker(ip,  k, cdevent, p));
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
                
                Console.WriteLine("Port " + port + " is open");
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
    }
}