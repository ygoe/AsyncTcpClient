using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unclassified.Net;

namespace AsyncTcpClientDemo
{
	public class Program
	{
		public static void Main(string[] args)
		{
			new Program().RunAsync().GetAwaiter().GetResult();
			Console.WriteLine("Press any key to exit...");
			Console.ReadKey(true);
		}

		/// <summary>
		/// Demonstrates the client and server with derived classes.
		/// </summary>
		/// <returns></returns>
		private async Task RunAsync()
		{
			int port = 7777;

			var server = new AsyncTcpListener<DemoTcpServerClient>
			{
				IPAddress = IPAddress.IPv6Any,
				Port = port
			};
			server.Message += (s, a) => Console.WriteLine("Server: " + a.Message);
			var serverTask = server.RunAsync();

			var client = new DemoTcpClient
			{
				IPAddress = IPAddress.IPv6Loopback,
				Port = port,
				//AutoReconnect = true
			};
			client.Message += (s, a) => Console.WriteLine("Client: " + a.Message);
			var clientTask = client.RunAsync();

			await Task.Delay(10000);
			Console.WriteLine("Program: stopping server");
			server.Stop(true);
			await serverTask;

			client.Dispose();
			await clientTask;
		}

		/// <summary>
		/// Demonstrates the client and server by using the classes directly with callback methods.
		/// </summary>
		/// <returns></returns>
		private async Task RunAsync2()
		{
			int port = 7777;

			var server = new AsyncTcpListener
			{
				IPAddress = IPAddress.IPv6Any,
				Port = port,
				ClientConnectedCallback = tcpClient =>
					new AsyncTcpClient
					{
						ServerTcpClient = tcpClient,
						ConnectedCallback = async (serverClient, isReconnected) =>
						{
							await Task.Delay(500);
							byte[] bytes = Encoding.UTF8.GetBytes("Hello, my name is Server. Talk to me.");
							await serverClient.Send(new ArraySegment<byte>(bytes, 0, bytes.Length));
						},
						ReceivedCallback = async (serverClient, count) =>
						{
							byte[] bytes = serverClient.ByteBuffer.Dequeue(count);
							string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
							Console.WriteLine("Server client: received: " + message);

							bytes = Encoding.UTF8.GetBytes("You said: " + message);
							await serverClient.Send(new ArraySegment<byte>(bytes, 0, bytes.Length));
						}
					}.RunAsync()
			};
			server.Message += (s, a) => Console.WriteLine("Server: " + a.Message);
			var serverTask = server.RunAsync();

			var client = new AsyncTcpClient
			{
				IPAddress = IPAddress.IPv6Loopback,
				Port = port,
				//AutoReconnect = true,
				ConnectedCallback = async (c, isReconnected) =>
				{
					await c.WaitAsync();   // Wait for server banner
					await Task.Delay(50);   // Let the banner land in the console window
					Console.WriteLine("Client: type a message at the prompt, or empty to quit (server shutdown in 10s)");
					while (true)
					{
						Console.Write("> ");
						var consoleReadCts = new CancellationTokenSource();
						var consoleReadTask = ConsoleEx.ReadLineAsync(consoleReadCts.Token);

						// Wait for user input or closed connection
						var completedTask = await Task.WhenAny(consoleReadTask, c.ClosedTask);
						if (completedTask == c.ClosedTask)
						{
							// Closed connection
							consoleReadCts.Cancel();
							break;
						}

						// User input
						string enteredMessage = await consoleReadTask;
						if (enteredMessage == "")
						{
							c.Dispose();
							break;
						}
						byte[] bytes = Encoding.UTF8.GetBytes(enteredMessage);
						await c.Send(new ArraySegment<byte>(bytes, 0, bytes.Length));

						// Wait for server response or closed connection
						await c.ByteBuffer.WaitAsync();
						if (c.IsClosing)
						{
							break;
						}
					}
				},
				ReceivedCallback = (c, count) =>
				{
					byte[] bytes = c.ByteBuffer.Dequeue(count);
					string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
					Console.WriteLine("Client: received: " + message);
					return Task.CompletedTask;
				}
			};
			client.Message += (s, a) => Console.WriteLine("Client: " + a.Message);
			var clientTask = client.RunAsync();

			await Task.Delay(10000);
			Console.WriteLine("Program: stopping server");
			server.Stop(true);
			await serverTask;

			client.Dispose();
			await clientTask;
		}
	}
}
