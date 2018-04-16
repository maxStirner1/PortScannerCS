using System;
using Microsoft.VisualBasic.CompilerServices;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace PortScanner
{
    public partial class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 4)
            {
                Usage();
                return;
            }
            if (CheckIpAddress(args[0],args[1]) &&
            CheckNumberOfHosts(args[2]) &&
            CheckNumberOfPorts(args[3]) == false) return;
            List<IPAddress> listOfIpAddresses = PopulateListOfIpAddresses(args[0], args[1]);
            StartScanner(listOfIpAddresses, args[2], args[3]);
        }
       
        static bool CheckIpAddress(string ip1, string ip2)
        {
            byte[] bytes;
            
            try
            {
                IPAddress ipadd1 = IPAddress.Parse(ip1);
                bytes = ipadd1.GetAddressBytes();
                for (int i = 0; i < 4; i++)
                {
                    if ((bytes[i] < 0) || (bytes[i] > 255))
                    {
                        Usage();
                        return false;
                    }
                }
                
                IPAddress ipadd2 = IPAddress.Parse(ip2);
                bytes = ipadd2.GetAddressBytes();
                for (int i = 0; i < 4; i++)
                {
                    if ((bytes[i] < 0) || (bytes[i] > 255))
                    {
                        Usage();
                        return false;
                    }
                }

                if ((uint) IPAddress.NetworkToHostOrder(BitConverter.ToInt32(ipadd1.GetAddressBytes(), 0))
                    > (uint) IPAddress.NetworkToHostOrder(BitConverter.ToInt32(ipadd2.GetAddressBytes(), 0)))
                {
                    Usage();
                    return false;
                }
            }

            catch(Exception e)
            {
                Console.WriteLine("Exception caught!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
                Usage();
                return false;
            }
            
            return true;
        }

        static bool CheckNumberOfHosts(string nrofhosts)
        {
            try
            {
                int numberOfHosts = Int32.Parse(nrofhosts);
                if (numberOfHosts > 1000)
                {
                    Usage();
                    return true;
                }    
            }
            
            catch(Exception e)
            {
                Console.WriteLine("Exception caught!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
                Usage();
                return false;
            }

            return true;
        }
        
        static bool CheckNumberOfPorts(string nrofports)
        {
            try
            {
                int numberOfPorts = Int32.Parse(nrofports);
                if (numberOfPorts > 1000)
                {
                    Usage();
                    return false;
                }    
            }
            
            catch(Exception e)
            {
                Console.WriteLine("Exception caught!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
                Usage();
                return false;
            }

            return true;
        }

        static List<IPAddress> PopulateListOfIpAddresses(string starting, string ending)
        {
            IPAddress ip1 = IPAddress.Parse(starting);
            IPAddress ip2 = IPAddress.Parse(ending);
            
            List<IPAddress> listOfIpAddresses = new List<IPAddress>();

            uint start = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(ip1.GetAddressBytes(), 0));
            uint end = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(ip2.GetAddressBytes(), 0));
            
            for (uint i = start; i <= end; i++)
            {
                byte[] bytes = BitConverter.GetBytes(i);
                listOfIpAddresses.Add(new IPAddress(new[] { bytes[3], bytes[2], bytes[1], bytes[0] }));
            }

            return listOfIpAddresses;
        }
        
        static void Usage()
        {
            Console.WriteLine(@"Please enter the starting address as first argument and
the ending address as second argument in IP format ie. 
192.168.0.1. The second IP address must be equal to or 
greater than the first. Enter the number of hosts to be 
scanned in parallel as third argument and the number of 
ports to be scanned in parallel on a host as fourth argument.
The numbers cannot be greater than 10 hosts and 300 threads 
per host");
        }
    }
}

