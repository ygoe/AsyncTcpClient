using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AsyncTcpClientDemo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
	[TestClass]
	public class ByteBufferTests
	{
		[TestMethod]
		public void EnqueueOne()
		{
			// Arrange
			var buffer = new ByteBuffer();

			// Act
			buffer.Enqueue(new byte[] { 1 });

			// Assert
			Assert.AreEqual(1, buffer.Count);
			Assert.AreEqual(1, buffer.Dequeue(1)[0]);
		}

		[TestMethod]
		public void EnqueueMany()
		{
			// Arrange
			var buffer = new ByteBuffer();

			// Act
			buffer.Enqueue(new byte[] { 1, 2, 3 });

			// Assert
			Assert.AreEqual(3, buffer.Count);
			byte[] dequeued = buffer.Dequeue(3);
			CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, dequeued);
		}

		[TestMethod]
		public void EnqueuePartial()
		{
			// Arrange
			var buffer = new ByteBuffer();

			// Act
			buffer.Enqueue(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, 2, 3);

			// Assert
			Assert.AreEqual(3, buffer.Count);
			byte[] dequeued = buffer.Dequeue(3);
			CollectionAssert.AreEqual(new byte[] { 3, 4, 5 }, dequeued);
		}

		[TestMethod]
		public void EnqueueSegment()
		{
			// Arrange
			var buffer = new ByteBuffer();
			var segment = new ArraySegment<byte>(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, 3, 4);

			// Act
			buffer.Enqueue(segment);

			// Assert
			Assert.AreEqual(4, buffer.Count);
			byte[] dequeued = buffer.Dequeue(4);
			CollectionAssert.AreEqual(new byte[] { 4, 5, 6, 7 }, dequeued);
		}

		[TestMethod]
		public void Full()
		{
			// Arrange
			var buffer = new ByteBuffer(4);

			// Act
			buffer.Enqueue(new byte[] { 1, 2, 3, 4 });

			// Assert
			Assert.AreEqual(4, buffer.Count);
			Assert.AreEqual(4, buffer.Capacity);
			byte[] dequeued = buffer.Dequeue(4);
			CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, dequeued);
		}

		[TestMethod]
		public void FullOverflow()
		{
			// Arrange
			var buffer = new ByteBuffer(4);
			buffer.Enqueue(new byte[] { 1, 2, 3 });
			buffer.Dequeue(3);

			// Act
			buffer.Enqueue(new byte[] { 4, 5, 6, 7 });

			// Assert
			Assert.AreEqual(4, buffer.Count);
			Assert.AreEqual(4, buffer.Capacity);
			byte[] dequeued = buffer.Dequeue(4);
			CollectionAssert.AreEqual(new byte[] { 4, 5, 6, 7 }, dequeued);
		}

		[TestMethod]
		public void DequeueToEmpty()
		{
			// Arrange
			var buffer = new ByteBuffer();
			buffer.Enqueue(new byte[] { 1, 2, 3 });

			// Act
			buffer.Dequeue(3);

			// Assert
			Assert.AreEqual(0, buffer.Count);
		}

		[TestMethod]
		public void Clear()
		{
			// Arrange
			var buffer = new ByteBuffer();
			buffer.Enqueue(new byte[] { 1, 2, 3 });

			// Act
			buffer.Clear();

			// Assert
			Assert.AreEqual(0, buffer.Count);
		}

		[TestMethod]
		public void EnqueueOneOverflow()
		{
			// Arrange
			var buffer = new ByteBuffer(4);
			buffer.Enqueue(new byte[] { 1, 2, 3, 4 });
			buffer.Dequeue(2);

			// Act
			buffer.Enqueue(new byte[] { 5 });

			// Assert
			Assert.AreEqual(3, buffer.Count);
			byte[] dequeued = buffer.Dequeue(3);
			CollectionAssert.AreEqual(new byte[] { 3, 4, 5 }, dequeued);
		}

		[TestMethod]
		public void EnqueueManyOverflow()
		{
			// Arrange
			var buffer = new ByteBuffer(4);
			buffer.Enqueue(new byte[] { 1, 2, 3, 4 });
			buffer.Dequeue(2);

			// Act
			buffer.Enqueue(new byte[] { 5, 6 });

			// Assert
			Assert.AreEqual(4, buffer.Count);
			byte[] dequeued = buffer.Dequeue(4);
			CollectionAssert.AreEqual(new byte[] { 3, 4, 5, 6 }, dequeued);
		}

		[TestMethod]
		public void GetBuffer()
		{
			// Arrange
			var buffer = new ByteBuffer(4);
			buffer.Enqueue(new byte[] { 1, 2, 3, 4 });
			buffer.Dequeue(2);
			buffer.Enqueue(new byte[] { 5, 6 });

			// Act
			byte[] array = buffer.Buffer;

			// Assert
			Assert.AreEqual(4, array.Length);
			CollectionAssert.AreEqual(new byte[] { 3, 4, 5, 6 }, array);
		}

		[TestMethod]
		public void EnqueueManyExtend()
		{
			// Arrange
			var buffer = new ByteBuffer(4);
			buffer.Enqueue(new byte[] { 1, 2, 3 });

			// Act
			buffer.Enqueue(new byte[] { 4, 5, 6 });

			// Assert
			Assert.AreEqual(8, buffer.Capacity);
			Assert.AreEqual(6, buffer.Count);
			byte[] dequeued = buffer.Dequeue(6);
			CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6 }, dequeued);
		}

		[TestMethod]
		public void EnqueueManySetCapacity()
		{
			// Arrange
			var buffer = new ByteBuffer(4);
			buffer.Enqueue(new byte[] { 1, 2, 3 });

			// Act
			buffer.SetCapacity(6);
			buffer.Enqueue(new byte[] { 4, 5, 6 });

			// Assert
			Assert.AreEqual(6, buffer.Capacity);
			Assert.AreEqual(6, buffer.Count);
			byte[] dequeued = buffer.Dequeue(6);
			CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6 }, dequeued);
		}

		[TestMethod]
		public async Task DequeueAsync()
		{
			// Arrange
			var buffer = new ByteBuffer();

			// Act
			var unawaited = Task.Run(async () =>
			{
				await Task.Delay(200);
				buffer.Enqueue(new byte[] { 1 });
				await Task.Delay(200);
				buffer.Enqueue(new byte[] { 2 });
				await Task.Delay(200);
				buffer.Enqueue(new byte[] { 3, 4 });
			});
			var sw = new Stopwatch();
			sw.Start();
			byte[] dequeued = await buffer.DequeueAsync(3);
			byte[] dequeued2 = await buffer.DequeueAsync(1);
			sw.Stop();

			// Assert
			Assert.AreEqual(3, dequeued.Length);
			Assert.AreEqual(1, dequeued[0]);
			Assert.AreEqual(2, dequeued[1]);
			Assert.AreEqual(3, dequeued[2]);
			Assert.AreEqual(1, dequeued2.Length);
			Assert.AreEqual(4, dequeued2[0]);
			Assert.IsTrue(sw.ElapsedMilliseconds > 550);
			Assert.IsTrue(sw.ElapsedMilliseconds < 700);
		}
	}
}
