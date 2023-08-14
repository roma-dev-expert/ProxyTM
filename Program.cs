using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ProxyServer
{
    class Program
    {
        static async Task Main()
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
            using var clientStream = client.GetStream();
            using var reader = new StreamReader(clientStream);
            using var writer = new StreamWriter(clientStream);

            var requestLine = await reader.ReadLineAsync();
            var parts = requestLine?.Split(' ');

            if (parts?.Length >= 3)
            {
                var method = parts[0];
                var url = parts[1] == "/" ? "/ru" : parts[1];

                if (method == "GET")
                {
                    using var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync($"{targetSite}{url}");

                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    var isTextHtml = contentType != null && contentType.Contains("text/html");

                    var modifiedContent = await ModifyContentAsync(response, isTextHtml);
                    await WriteResponseAsync(writer, modifiedContent, contentType);
                }
            }

            client.Close();
        }

        static bool IsInsideScriptTag(HtmlNode node)
        {
            while (node != null)
            {
                if (node.Name.Equals("script", StringComparison.OrdinalIgnoreCase)) return true;

                node = node.ParentNode;
            }

            return false;
        }

        static async Task<string> ModifyContentAsync(HttpResponseMessage response, bool isTextHtml)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (isTextHtml)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                foreach (var textNode in doc.DocumentNode.DescendantsAndSelf())
                {
                    if (textNode.NodeType == HtmlNodeType.Text && !IsInsideScriptTag(textNode))
                    {
                        var modifiedText = Regex.Replace(textNode.InnerText, @"\b\w{6}\b", match =>
                        {
                            return match.Value + "™";
                        });

                        textNode.InnerHtml = modifiedText;
                    }
                }

                content = doc.DocumentNode.OuterHtml;
            }

            return content;
        }

        static async Task WriteResponseAsync(StreamWriter writer, string content, string contentType)
        {
            await writer.WriteLineAsync("HTTP/1.1 200 OK");
            await writer.WriteLineAsync($"Content-Length: {content.Length}");
            await writer.WriteLineAsync("Connection: close");
            await writer.WriteLineAsync($"Content-type: {contentType}");
            await writer.WriteLineAsync();
            await writer.WriteAsync(content);
        }
    }
}
