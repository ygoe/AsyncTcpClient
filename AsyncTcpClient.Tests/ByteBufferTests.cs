using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unclassified.Util;

namespace AsyncTcpClient.Tests
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
		public void Peek()
		{
			// Arrange
			var buffer = new ByteBuffer();
			buffer.Enqueue(new byte[] { 1, 2, 3, 4 });

			// Act
			byte[] peeked = buffer.Peek(2);

			// Assert
			CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, buffer.Buffer);
			CollectionAssert.AreEqual(new byte[] { 1, 2 }, peeked);

			// Act 2
			byte[] dequeued = buffer.Dequeue(3);

			// Assert 2
			CollectionAssert.AreEqual(new byte[] { 4 }, buffer.Buffer);
			CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, dequeued);
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
			CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, dequeued);
			CollectionAssert.AreEqual(new byte[] { 4 }, dequeued2);
			Assert.IsTrue(sw.ElapsedMilliseconds > 550);
			Assert.IsTrue(sw.ElapsedMilliseconds < 700);
		}

		[TestMethod]
		public async Task DequeueAsyncCancel()
		{
			// Arrange
			var buffer = new ByteBuffer();

			// Act & Assert
			var unawaited = Task.Run(async () =>
			{
				await Task.Delay(200);
				buffer.Enqueue(new byte[] { 1 });
				await Task.Delay(200);
				buffer.Enqueue(new byte[] { 2 });
			});
			byte[] dequeued = null;
			var sw = new Stopwatch();
			sw.Start();
			await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
			{
				var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
				dequeued = await buffer.DequeueAsync(3, cts.Token);
			});
			sw.Stop();
			Assert.IsNull(dequeued);
			Assert.IsTrue(sw.ElapsedMilliseconds > 950);
			Assert.IsTrue(sw.ElapsedMilliseconds < 1250);
		}

		[TestMethod]
		public void Enqueue_Should_Resize_Buffer_To_Length_Of_Payload_If_Payload_Length_Exceeds_Twice_Capacity()
		{
			// Arrange
			const string largePayload = "amet consectetur adipiscing elit ut aliquam purus sit amet luctus venenatis lectus magna fringilla urna porttitor rhoncus dolor purus non enim praesent elementum facilisis leo vel fringilla est ullamcorper eget nulla facilisi etiam dignissim diam quis enim lobortis scelerisque fermentum dui faucibus in ornare quam viverra orci sagittis eu volutpat odio facilisis mauris sit amet massa vitae tortor condimentum lacinia quis vel eros donec ac odio tempor orci dapibus ultrices in iaculis nunc sed augue lacus viverra vitae congue eu consequat ac felis donec et odio pellentesque diam volutpat commodo sed egestas egestas fringilla phasellus faucibus scelerisque eleifend donec pretium vulputate sapien nec sagittis aliquam malesuada bibendum arcu vitae elementum curabitur vitae nunc sed velit dignissim sodales ut eu sem integer vitae justo eget magna fermentum iaculis eu non diam phasellus vestibulum lorem sed risus ultricies tristique nulla aliquet enim tortor at auctor urna nunc id cursus metus aliquam eleifend mi in nulla posuere sollicitudin aliquam ultrices sagittis orci a scelerisque purus semper eget duis at tellus at urna condimentum mattis pellentesque id nibh tortor id aliquet lectus proin nibh nisl condimentum id venenatis a condimentum vitae sapien pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis amet consectetur adipiscing elit ut aliquam purus sit amet luctus venenatis lectus magna fringilla urna porttitor rhoncus dolor purus non enim praesent elementum facilisis leo vel fringilla est ullamcorper eget nulla facilisi etiam dignissim diam quis enim lobortis scelerisque fermentum dui faucibus in ornare quam viverra orci sagittis eu volutpat odio facilisis mauris sit amet massa vitae tortor condimentum lacinia quis vel eros donec ac odio tempor orci dapibus ultrices in iaculis nunc sed augue lacus viverra vitae congue eu consequat ac felis donec et odio pellentesque diam volutpat commodo sed egestas egestas fringilla phasellus faucibus scelerisque eleifend donec pretium vulputate sapien nec sagittis aliquam malesuada bibendum arcu vitae elementum curabitur vitae nunc sed velit dignissim sodales ut eu sem integer vitae justo eget magna fermentum iaculis eu non diam phasellus vestibulum lorem sed risus ultricies tristique nulla aliquet enim tortor at auctor urna nunc id cursus metus aliquam eleifend mi in nulla posuere sollicitudin aliquam ultrices sagittis orci a scelerisque purus semper eget duis at tellus at urna condimentum mattis pellentesque id nibh tortor id aliquet lectus proin nibh nisl condimentum id venenatis a condimentum vitae sapien pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis amet consectetur adipiscing elit ut aliquam purus sit amet luctus venenatis lectus magna fringilla urna porttitor rhoncus dolor purus non enim praesent elementum facilisis leo vel fringilla est ullamcorper eget nulla facilisi etiam dignissim diam quis enim lobortis scelerisque fermentum dui faucibus in ornare quam viverra orci sagittis eu volutpat odio facilisis mauris sit amet massa vitae tortor condimentum lacinia quis vel eros donec ac odio tempor orci dapibus ultrices in iaculis nunc sed augue lacus viverra vitae congue eu consequat ac felis donec et odio pellentesque diam volutpat commodo sed egestas egestas fringilla phasellus faucibus scelerisque eleifend donec pretium vulputate sapien nec sagittis aliquam malesuada bibendum arcu vitae elementum curabitur vitae nunc sed velit dignissim sodales ut eu sem integer vitae justo eget magna fermentum iaculis eu non diam phasellus vestibulum lorem sed risus ultricies tristique nulla aliquet enim tortor at auctor urna nunc id cursus metus aliquam eleifend mi in nulla posuere sollicitudin aliquam ultrices sagittis orci a scelerisque purus semper eget duis at tellus at urna condimentum mattis pellentesque id nibh tortor id aliquet lectus proin nibh nisl condimentum id venenatis a condimentum vitae sapien pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis";
			byte[] payloadBytes = Encoding.UTF8.GetBytes(largePayload);
			var buffer = new ByteBuffer();

			// Act
			buffer.Enqueue(payloadBytes);

			// Assert
			Assert.AreEqual(payloadBytes.Length, buffer.Count);
			CollectionAssert.AreEqual(payloadBytes, buffer.Dequeue(buffer.Count));
		}
	}
}
