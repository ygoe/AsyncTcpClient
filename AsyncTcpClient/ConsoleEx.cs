using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTcpClientDemo
{
	/// <summary>
	/// Provides a cancellable version of <see cref="Console.ReadLine"/>.
	/// </summary>
	public static class ConsoleEx
	{
		public static async Task<string> ReadLineAsync(CancellationToken cancellationToken)
		{
			string message = "";
			while (true)
			{
				while (!Console.KeyAvailable)
				{
					cancellationToken.ThrowIfCancellationRequested();
					await Task.Delay(10);
				}
				var keyInfo = Console.ReadKey(true);
				switch (keyInfo.KeyChar)
				{
					case '\0':
						break;
					case '\b':
						// Delete previous character
						if (message.Length > 0)
						{
							message = message.Substring(0, message.Length - 1);
							if (Console.CursorLeft > 0)
							{
								Console.CursorLeft--;
								Console.Write(" ");
								Console.CursorLeft--;
							}
						}
						break;
					case '\r':
						// Return key, execute command
						Console.WriteLine();
						return message;
					default:
						// Input character
						message += keyInfo.KeyChar;
						Console.Write(keyInfo.KeyChar);
						break;
				}
			}
		}
	}
}
