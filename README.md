# AsyncTcpClient & AsyncTcpListener

*An asynchronous variant of TcpClient and TcpListener for .NET Standard.*

[![NuGet](https://img.shields.io/nuget/v/Unclassified.AsyncTcpClient.svg)](https://www.nuget.org/packages/Unclassified.AsyncTcpClient)

Building asynchronous solutions with TcpClient and TcpListener is complicated and it is easy to introduce bugs or miss critical features. These classes provide an easy solution for this task. Writing asynchronous TCP/IP clients and servers with these classes only requires the implementation of very basic callbacks. Alternatively, you can implement your connection logic in derived classes. In any case, you just have to read the received data from a buffer and send data back. All socket and stream interaction is hidden from you.

This also includes an **asynchronous byte buffer** that keeps all received bytes as they come in. Applications can dequeue as many bytes as they need without discarding what else was received. Dequeueing returns up to as many bytes as are available when called synchronously, or exactly the requested number of bytes when called asynchronously. The async method can be cancelled. This ensures that the code will never block irrecoverably.

A complete example of both implementation styles is provided in the application in this repository. Start reading at [Program.cs](https://github.com/ygoe/AsyncTcpClient/blob/master/AsyncTcpClientDemo/Program.cs).

## Features

* Awaitable (async) client connection
* Awaitable (async) listener
* Automatic reconnecting of the client on connection loss
* Client: React on new connection
* Client: React on closed connection
* Client: React on received data
* Client: Received data queue buffer, fetch as much data as you need when it’s available
* Just three small class files to add to your project ([AsyncTcpClient](https://github.com/ygoe/AsyncTcpClient/blob/master/AsyncTcpClient/AsyncTcpClient.cs), [ByteBuffer](https://github.com/ygoe/AsyncTcpClient/blob/master/AsyncTcpClient/ByteBuffer.cs), optional: [AsyncTcpListener](https://github.com/ygoe/AsyncTcpClient/blob/master/AsyncTcpClient/AsyncTcpListener.cs))

## What’s so hard about TcpClient?

“The [TcpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient) class provides simple methods for connecting, sending, and receiving stream data over a network in synchronous blocking mode.” Reading from the network stream is a blocking operation. Once you decided that you need received data, your code will block until data was actually received. Your application will even deadlock if no data will ever be sent for some reason. While you can use the DataAvailable property to test that, you still need to poll it regularly, introducing additional load and delays.

While you’re doing something else and not wait for received data, you will also not notice when the connection was closed remotely. Detecting this requires calling the Read method.

Add to this exception handling with special error codes for normal situations and things are getting complicated.

AsyncTcpClient and AsyncTcpListener take all that complexity out of the process, with the bonus of automatic client reconnecting and server-side client connection management. You just handle a new connection and/or received data, the rest is taken care of by these classes.

## Client side

### Usage

Derived class with overridden methods:

    var client = new DemoTcpClient
    {
        IPAddress = IPAddress.IPv6Loopback,
        Port = 12345,
        //AutoReconnect = true
    };
    await client.RunAsync();

AsyncTcpClient with callbacks:

    var client = new AsyncTcpClient
    {
        IPAddress = IPAddress.IPv6Loopback,
        Port = 12345,
        //AutoReconnect = true,
        ConnectedCallback = async (client, isReconnected) =>
        {
            // Custom connection logic
        },
        ReceivedCallback = async (client, count) =>
        {
            // Custom connection logic
        }
    };
    await client.RunAsync();

Close the client connection:

    // This can be called from within a callback
	client.Disconnect();
	
	// This should only be called outside of RunAsync
    client.Dispose();

### OnConnected

Virtual method to override in derived classes:

    protected virtual Task OnConnectedAsync(bool isReconnected)

Callback method:

    public Func<AsyncTcpClient, bool, Task> ConnectedCallback { get; set; }

Called when the client has connected to the remote host. This method can implement the communication logic to execute when the connection was established. The connection will not be closed before this method completes.

### OnClosed

Virtual method to override in derived classes:

    protected virtual void OnClosed(bool remote)

Callback method:

    public Action<AsyncTcpClient, bool> ClosedCallback { get; set; }

Called when the connection was closed.

Parameter *remote*: true, if the connection was closed by the remote host; false, if the connection was closed locally.

### OnReceived

Virtual method to override in derived classes:

    protected virtual Task OnReceivedAsync(int count)

Callback method:

    public Func<AsyncTcpClient, int, Task> ReceivedCallback { get; set; }

Called when data was received from the remote host. This method can implement the communication logic to execute every time data was received. New data will not be received before this method completes.

Parameter *count*: The number of bytes that were received. The actual data is available through the ByteBuffer.

## Server side

### Usage

Generic class with type argument for derived client class:

    var server = new AsyncTcpListener<DemoTcpServerClient>
    {
        IPAddress = IPAddress.IPv6Any,
        Port = 12345
    };
    await server.RunAsync();

Non-generic AsyncTcpListener with callbacks:

    var server = new AsyncTcpListener
    {
        IPAddress = IPAddress.IPv6Any,
        Port = 12345,
        ClientConnectedCallback = tcpClient =>
            new AsyncTcpClient
            {
                ServerTcpClient = tcpClient,
                RemoteEndPoint = tcpClient.Client.RemoteEndPoint,
                ConnectedCallback = async (serverClient, isReconnected) =>
                {
                    // Custom server logic
                },
                ReceivedCallback = async (serverClient, count) =>
                {
                    // Custom server logic
                }
            }.RunAsync()
    };
    await server.RunAsync();

Stop the listener:

    server.Stop(true);

Stop the listener but keep client connections open:

    server.Stop(false);

### OnClientConnected

Virtual method to override in derived classes:

    protected virtual Task OnClientConnected(TcpClient tcpClient)

Callback method:

    public Func<TcpClient, Task> ClientConnectedCallback { get; set; }

Called when a pending connection request was accepted. When this method completes, the client connection will be closed.

Parameter *tcpClient*: The TcpClient that represents the accepted connection.

Generic type parameter:

    public class AsyncTcpListener<TClient>
        : AsyncTcpListener
        where TClient : AsyncTcpClient, new()

Instantiates a new AsyncTcpClient instance of the type TClient that runs the accepted connection. This implementation does not call the OnClientConnected callback method.

## License

Permissive license

Copyright (c) 2018, Yves Goergen, https://unclassified.software

Copying and distribution of this file, with or without modification, are permitted provided the copyright notice and this notice are preserved. This file is offered as-is, without any warranty.
