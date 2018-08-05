using System;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTcpClientDemo
{
	public class DemoTcpServerClient : AsyncTcpClient
	{
		public DemoTcpServerClient()
		{
			Message += (s, a) => Console.WriteLine("Server client: " + a.Message);
		}

		protected override async Task OnConnectedAsync(bool isReconnected)
		{
			await Task.Delay(500);
			byte[] bytes = Encoding.UTF8.GetBytes("Hello, my name is Server. Talk to me.");
			await Send(new ArraySegment<byte>(bytes, 0, bytes.Length));
		}

		protected override async Task OnReceivedAsync(int count)
		{
			byte[] bytes = ByteBuffer.Dequeue(count);
			string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
			Console.WriteLine("Server client: received: " + message);

			bytes = Encoding.UTF8.GetBytes("You said: " + message);
			await Send(new ArraySegment<byte>(bytes, 0, bytes.Length));
		}
	}
}
