# C# .NET Proxy Server

This C# .NET proxy server is designed to modify page content and change internal links, providing intermediate data processing between the client and the server.

## Getting Started

1. Download the code from this repository to your local machine.
2. Open the project in Visual Studio or another C# .NET development environment.
3. Run the application.
4. Open http://localhost:8080/ 
 

## How It Works
1. The proxy server starts on port 8080.
2. When a client connects, the proxy server awaits an HTTP request.
3. If the request method is "GET," the server downloads a page from the specified website and modifies the text and links according to the assignment (To each word that consists of six letters, the symbol of “™” is added).
4. The proxy server sends the modified content to the client as an HTTP response.
5. The client closes.