using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTcpClientDemo
{
	public class DemoTcpClient : AsyncTcpClient
	{
		protected override async Task OnConnectedAsync(bool isReconnected)
		{
			await WaitAsync();   // Wait for server banner
			await Task.Delay(50);   // Let the banner land in the console window
			Console.WriteLine("Client: type a message at the prompt, or empty to quit (server shutdown in 10s)");
			while (true)
			{
				Console.Write("> ");
				var consoleReadCts = new CancellationTokenSource();
				var consoleReadTask = ConsoleEx.ReadLineAsync(consoleReadCts.Token);

				// Wait for user input or closed connection
				var completedTask = await Task.WhenAny(consoleReadTask, ClosedTask);
				if (completedTask == ClosedTask)
				{
					// Closed connection
					consoleReadCts.Cancel();
					break;
				}

				// User input
				string enteredMessage = await consoleReadTask;
				if (enteredMessage == "")
				{
					Dispose();
					break;
				}
				byte[] bytes = Encoding.UTF8.GetBytes(enteredMessage);
				await Send(new ArraySegment<byte>(bytes, 0, bytes.Length));

				// Wait for server response or closed connection
				await ByteBuffer.WaitAsync();
				if (IsClosing)
				{
					break;
				}
			}
		}

		protected override Task OnReceivedAsync(int count)
		{
			byte[] bytes = ByteBuffer.Dequeue(count);
			string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
			Console.WriteLine("Client: received: " + message);
			return Task.CompletedTask;
		}
	}
}
