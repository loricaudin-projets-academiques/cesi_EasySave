using System.Net;
using System.Net.Sockets;
using System.Text;

public class Server
{
    private static Socket StartServer()
    {
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 5000);
        server.Bind(endPoint);

        server.Listen(5);
        Console.WriteLine("Serveur en cours d'exécution sur le port 5000...");

        return server;
    }

    private static Socket AcceptConnection(Socket socket)
    {
        Socket client = socket.Accept();

        IPEndPoint remoteEndPoint = (IPEndPoint)client.RemoteEndPoint;
        string clientIP = remoteEndPoint.Address.ToString();
        int clientPort = remoteEndPoint.Port;

        // Affichage
        Console.WriteLine($"Client connecté depuis {clientIP}:{clientPort}");

        return client;
    }

    private static void ListenToClient(Socket client)
    {
        try
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                // Réception du message
                int received = client.Receive(buffer);

                // Si received == 0, le client s'est déconnecté proprement
                if (received == 0)
                {
                    Console.WriteLine("Client déconnecté.");
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, received);

                string response = MessageReceived(message);
                byte[] data = Encoding.UTF8.GetBytes(response);

                client.Send(data);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Connexion perdue avec le client : {e.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    private static string MessageReceived(string message)
    {
        Console.WriteLine(message);
        return "Ca va ?";
    }

    private static string GetLogPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return "log.txt";
        }
        else
        {
            // Environnement Docker/Linux
            return "/app/logs/log.txt";
        }
    }

    private static void Disconnect(Socket socket)
    {
        try
        {
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                Console.WriteLine("Connexion fermée.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Erreur lors de la fermeture du socket : " + e.Message);
        }
    }


    static void Main(string[] args)
    {
        Socket serverSocket = StartServer();
        try
        {
            while (true)
            {
                Socket clientSocket = AcceptConnection(serverSocket);

                ListenToClient(clientSocket);

                Disconnect(clientSocket);
            }
        }
        finally
        {
            Disconnect(serverSocket);
        }
    }
}
