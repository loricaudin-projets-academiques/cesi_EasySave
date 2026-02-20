using EasyLog.Configuration;
using EasyLog.Models;
using EasyLog.Server;
using EasyLog.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

public class Server
{
    static ServerConfigurations serverConfigurations = ServerConfigurations.GetInstance();

    static LogConfiguration logConfiguration = new LogConfiguration();
    static EasyLogger logger = new EasyLogger(logConfiguration);

    private static Socket StartServer()
    {
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        int port = serverConfigurations.Port;

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
        server.Bind(endPoint);

        server.Listen(5);
        Console.WriteLine($"Serveur en cours d'exécution sur le port {port}...");

        return server;
    }

    private static Socket AcceptConnection(Socket socket)
    {
        Socket client = socket.Accept();

        IPEndPoint remoteEndPoint = (IPEndPoint)client.RemoteEndPoint;
        Console.WriteLine($"Client connecté depuis {remoteEndPoint.Address}:{remoteEndPoint.Port}");

        return client;
    }

    private static void ListenToClient(Socket client)
    {
        try
        {
            byte[] buffer = new byte[4096];

            while (true)
            {
                int received = client.Receive(buffer);

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
        try
        {
            Console.WriteLine($"1 recpetion : {message}\n");
            using JsonDocument doc = JsonDocument.Parse(message);
            Console.WriteLine($"{message} OK");
            JsonElement root = doc.RootElement;

            string type = root.GetProperty("Type").GetString();
            JsonElement data = root.GetProperty("Data");

            switch (type)
            {
                case "LogEntry":
                    HandleLogEntry(data);
                    break;

                case "StateEntry":
                    HandleStateEntry(data);
                    break;

                default:
                    throw new Exception($"Type inconnu : {type}");
            }

            return "Success";
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e}");
            return $"Error: {e}";
        }
    }

    private static void HandleLogEntry(JsonElement data)
    {
        var log = JsonSerializer.Deserialize<LogEntry>(data);

        logger.LogFileTransfer(
            log.Name,
            log.FileSource,
            log.FileTarget,
            log.FileSize,
            log.FileTransferTime,
            log.EncryptionTime,
            log.BackupType
        );

        Console.WriteLine("LogEntry traité.");
    }

    private static void HandleStateEntry(JsonElement data)
    {
        var state = JsonSerializer.Deserialize<StateEntry>(data);

        logger.StartBackup(
            state.WorkIndex,
            state.Name,
            state.TotalFilesToCopy,
            state.TotalFilesSize
        );

        Console.WriteLine("StateEntry traité.");
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
