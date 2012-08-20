using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Xml.Linq;

namespace UdpServer
{
    public class Servidor
    {        
        List<string> instrucciones;
        public bool stopListening = false;
        public bool stopSending = false;
        public string message = "";

        public string dataReceived = null;

        private int port = 6767; //Port for the Server to use

        private string ip = "127.0.0.1";//IP Address 127.0.0.1

        public UdpClient server;

        string clientAdress;

        int clientPort;

        public IPEndPoint receivePoint;

        public Thread listener;

        public Thread sender;

        private void LoadConfigFile()
        {                                
            try
            {
                XDocument doc = XDocument.Load("ClientConfigFile.xml");                
                clientAdress = ((XElement)doc.Document.Element("Configuration").Element("Client").Nodes().ElementAt(0)).Value;
                clientPort = Convert.ToInt32(((XElement)doc.Document.Element("Configuration").Element("Client").Nodes().ElementAt(1)).Value);
                ip = ((XElement)doc.Document.Element("Configuration").Element("Server").Nodes().ElementAt(0)).Value; ;
                port = Convert.ToInt32(((XElement)doc.Document.Element("Configuration").Element("Server").Nodes().ElementAt(1)).Value);
            }
            catch
            {
                clientAdress = "localhost";
                clientPort = 6060;
            }
        }
        public string GetNextInstruction() {
            if (instrucciones.Count == 0)
            {
                return null;
            }
            string output = instrucciones[0];
            instrucciones.RemoveAt(0);
            return output;
        }
        public Servidor()
        {
            instrucciones = new List<string>();            
            LoadConfigFile();
            server = new UdpClient(port);
            receivePoint = new IPEndPoint(IPAddress.Any, port);
            listener = new Thread(new ThreadStart(listen_server));
            sender = new Thread(new ThreadStart(send_server));
        }
               
        public void start()
        {
            listener.Start();
            sender.Start();
        }

        public void sendMessageTo(string message)
        {            
            System.Text.ASCIIEncoding encode = new System.Text.ASCIIEncoding();
            string sendstring = message;
            byte[] sendData = encode.GetBytes(sendstring);
            server.Send(sendData, sendData.Length, clientAdress, clientPort);            
        }

        private void send_server()
        {
            while (true)
            {                     
                this.sendMessageTo(message);
                Thread.Sleep(10);
            }
        }
        public void Close() {

            listener.Abort();
            sender.Abort();
            server.Close();
            

        }
        private void listen_server()
        {
            while (true)
            {                
                try
                {
                    byte[] recData = server.Receive(ref receivePoint);                    
                    System.Text.ASCIIEncoding encode = new System.Text.ASCIIEncoding();
                    dataReceived = encode.GetString(recData);
                    if (dataReceived != null)
                    {
                        string[] data = dataReceived.Split('|');
                        foreach (string x in data) {
                            if (!String.IsNullOrEmpty(x)) {
                                instrucciones.Add(x);
                            }
                        }
                        dataReceived = null;
                        
                    }
                }
                catch
                {

                }
            }
        }

    }
}
