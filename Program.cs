using System.Net;
using System.Text.RegularExpressions;
using System.Net.Sockets;

namespace ProxyServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const string targetSite = "https://habr.com"; 
            var listener = new TcpListener(IPAddress.Any, 8080);

            listener.Start();
            Console.WriteLine("Proxy server started on port 8080...");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client, targetSite);
            }
        }

        static async Task HandleClientAsync(TcpClient client, string targetSite)
        {
            using (var clientStream = client.GetStream())
            {
                using (var reader = new StreamReader(clientStream))
                using (var writer = new StreamWriter(clientStream))
                {
                    var requestLine = await reader.ReadLineAsync();
                    var parts = requestLine?.Split(' ');

                    if (parts?.Length >= 3)
                    {
                        var method = parts[0];
                        var url = parts[1];

                        if (method == "GET")
                        {
                            var modifiedContent = await FetchAndModifyContentAsync(targetSite, url);
                            await writer.WriteLineAsync("HTTP/1.1 200 OK");
                            await writer.WriteLineAsync($"Content-Length: {modifiedContent.Length}");
                            await writer.WriteLineAsync("Connection: close");
                            await writer.WriteLineAsync();
                            await writer.WriteAsync(modifiedContent);
                        }
                    }
                }
            }

            client.Close();
        }

        static async Task<string> FetchAndModifyContentAsync(string targetSite, string url)
        {
            using (var webClient = new WebClient())
            {
                var content = await webClient.DownloadStringTaskAsync($"{targetSite}{url}");

                var modifiedContent = Regex.Replace(content, @"\b\w{6}\b", match =>
                {
                    return match.Value + "™";
                });

                modifiedContent = Regex.Replace(modifiedContent, @"https?://habr\.com", "http://localhost:8080");

                return modifiedContent;
            }
        }
    }
}
