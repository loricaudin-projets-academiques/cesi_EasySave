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
            string typeName = data.GetType().Name;
            var payload = new
            {
                Type = typeName,
                Data = data
            };

            string json = JsonSerializer.Serialize(payload);
            Console.WriteLine($"1 envoi : {json}\n");
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            _client.Send(bytes);
        }


        public void Close()
        {
            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }
    }
}
