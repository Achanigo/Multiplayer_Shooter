using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Transactions;

namespace UDPClient
{
    public class Cliente
    {
        public string gameInformation;
        public static UdpClient client;
        public static Socket socket;
        public static IPEndPoint receivePoint;
        public string message;

        public static int port;
        public static string ip;

        public static IPEndPoint destination;
        public static string ipDestination;//Ip del servidor
        public static int portDestination;        

        private Thread listener;
        private Thread sender;

        private void LoadConfigurationFile()
        {
            try
            {
                XDocument doc = XDocument.Load("ClientConfigFile.xml");

                ipDestination = ((XElement)doc.Document.Element("Configuration").Element("Server").Nodes().ElementAt(0)).Value;
                portDestination = Convert.ToInt32(((XElement)doc.Document.Element("Configuration").Element("Server").Nodes().ElementAt(1)).Value);

                ip = ((XElement)doc.Document.Element("Configuration").Element("Client").Nodes().ElementAt(0)).Value;
                port = Convert.ToInt32(((XElement)doc.Document.Element("Configuration").Element("Client").Nodes().ElementAt(1)).Value);
            }
            catch
            {                
                ipDestination = "127.0.0.1";
                portDestination = 6767;
                ip = "127.0.0.1";
                port = 6060;
            }
        }
        public Cliente()
        {
            gameInformation = "";
            LoadConfigurationFile();
            destination = new IPEndPoint(IPAddress.Parse(ipDestination), portDestination);
            client = new UdpClient(port);
            receivePoint = new IPEndPoint(IPAddress.Parse(ip), port);
            listener = new Thread(new ThreadStart(listener_client));
            sender = new Thread(new ThreadStart(sender_client));
            //this.Start();
        }

        public void Start()
        {
            sender.Start();
            listener.Start();
        }

        public void sender_client()
        {
            while (true)
            {

                using (TransactionScope t = new TransactionScope())
                {
                    if (this.message != null)
                    {
                        this.sendInstruction(message);
                        message = null;
                        Thread.Sleep(10);
                    }
                    t.Complete();
                }
            }
        }

        public void listener_client()
        {
            while (true)
            {
                try
                {
                    this.updateInformation();
                }
                catch { }
            }
        }
        private void updateInformation()
        {
            byte[] recData = client.Receive(ref receivePoint);
            System.Text.ASCIIEncoding encode = new System.Text.ASCIIEncoding();

            if (!string.IsNullOrEmpty(encode.GetString(recData)))
            {
                gameInformation = encode.GetString(recData);                
            }            
        }
        public void sendInstruction(string s)
        {
            System.Text.ASCIIEncoding encode = new System.Text.ASCIIEncoding();
            byte[] sendData = encode.GetBytes(s);
            //client.Send(sendData, sendData.Length, "localhost", 6767);// a quien manda informacion                    
            client.Send(sendData, sendData.Length, destination);// a quien manda informacion                    
        }
        public void Close()
        {
            listener.Abort();
            sender.Abort();
            client.Close();            
        }
    }

}
