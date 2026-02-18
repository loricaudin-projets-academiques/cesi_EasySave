using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace EasyLog.Services
{
    internal class EasyLogServer
    {
        private readonly Socket _client;

        public EasyLogServer(string ip, int port)
        {
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _client.Connect(ip, port);
        }

        public void SendJson(object data)
        {
            Console.WriteLine("OK");
            string json = JsonSerializer.Serialize(data);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            _client.Send(bytes);

            byte[] buffer = new byte[1024];
            int received = _client.Receive(buffer);

            if (received == 0)
            {
                Console.WriteLine("Serveur déconnecté.");
            }
            else
            {
                string response = Encoding.UTF8.GetString(buffer, 0, received);
                Console.WriteLine("Serveur : " + response);
            }
        }

        public void Close()
        {
            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }
    }
}
