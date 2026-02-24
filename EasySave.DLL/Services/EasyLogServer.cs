using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace EasyLog.Services
{
    internal class EasyLogServer
    {
        private Socket _client;
        private Mutex _mutex = new Mutex();

        public EasyLogServer()
        {
        }

        public bool ConnectToLogServer(string ip, int port)
        {
            try
            {
                _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _client.Connect(ip, port);
                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public void SendJson(object data)
        {
            string typeName = data.GetType().Name;
            var payload = new
            {
                Type = typeName,
                Data = data
            };

            string json = JsonSerializer.Serialize(payload) + "\n"; // \n pourra servir pour régler le problème d'envoi de plusieurs JSON pouvant se retrouver collé côté serveur.
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
