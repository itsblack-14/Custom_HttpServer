using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections.Generic;

public class Users
{
    public string Id { get; set; }
    public string Name { get; set; }
}

class Server
{
    static void Main(string[] args)
    {
        var users = new List<Users>
        {
            new Users{ Id = "1", Name = "John Doe" },
            new Users{ Id = "2", Name = "Jane Smith" }
        };

        int port = 8080; // Define the port to listen on
        TcpListener listener = new TcpListener(IPAddress.Any, port);

        try
        {
            listener.Start();
            Console.WriteLine($"Web Server running at http://localhost:{port}/");
            Console.WriteLine("Press Ctrl+C to stop the server.");

            while (true)
            {
                // Accept incoming client connection
                TcpClient client = listener.AcceptTcpClient();

                using (NetworkStream stream = client.GetStream())
                {
                    // Read the HTTP request
                    byte[] buffer = new byte[4096];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Parse the request
                    string[] requestLines = request.Split("\r\n");
                    string requestLine = requestLines[0]; // e.g., "GET /api/users HTTP/1.1"
                    string[] requestParts = requestLine.Split(' ');

                    if (requestParts.Length >= 2)
                    {
                        string method = requestParts[0]; // HTTP method 
                        string url = requestParts[1];    // URL path

                        string body = "";
                        int emptyLineIndex = Array.IndexOf(requestLines, "");
                        if (emptyLineIndex >= 0 && emptyLineIndex < requestLines.Length - 1)
                        {
                            body = string.Join("\r\n", requestLines[(emptyLineIndex + 1)..]);
                        }

                        // Handle routes
                        string responseBody;
                        int statusCode = 200; // Default to 200 OK

                        if (url == "/api/users" && method == "GET")
                        {
                            responseBody = System.Text.Json.JsonSerializer.Serialize(users);
                        }
                        else if (url.StartsWith("/api/users/") && method == "GET")
                        {
                            string id = url.Replace("/api/users/", "");
                            var user = users.FirstOrDefault(x => x.Id == id);

                            if (user != null)
                            {
                                responseBody = System.Text.Json.JsonSerializer.Serialize(user);
                            }
                            else
                            {
                                statusCode = 404;
                                responseBody = System.Text.Json.JsonSerializer.Serialize(new { Error = "User Not Found" } );
                            }
                        }
                        else if (url.StartsWith("/api/users/") && method == "POST" )
                        {
                            try
                            {
                                // Parse the incoming JSON body
                                var newUser = System.Text.Json.JsonSerializer.Deserialize<Users>(body);

                                if (newUser != null)
                                {
                                    users.Add(newUser);
                                    responseBody = System.Text.Json.JsonSerializer.Serialize(newUser);
                                }
                                else
                                {
                                    statusCode = 400; // Bad Request
                                    responseBody = System.Text.Json.JsonSerializer.Serialize(new { Error = "Invalid User Data" });
                                }
                            }
                            catch
                            {
                                statusCode = 400; // Bad Request
                                responseBody = System.Text.Json.JsonSerializer.Serialize(new { Error = "Malformed JSON" });
                            }
                        }
                        else
                        {
                            // 404 Not Found
                            statusCode = 404;
                            responseBody = System.Text.Json.JsonSerializer.Serialize(new { Error = "Not Found" });
                        }

                        // Prepare and send the response
                        string response =
                            $"HTTP/1.1 {statusCode} {(statusCode == 200 ? "OK" : "Not Found")}\r\n" +
                            "Content-Type: application/json\r\n" +
                            $"Content-Length: {responseBody.Length}\r\n" +
                            "\r\n" +
                            responseBody;

                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                    }
                }

                client.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            listener.Stop();
        }
    }
}