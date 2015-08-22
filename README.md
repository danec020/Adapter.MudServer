# Adapter.MudServer
A basic Mud Engine server for Windows and OS X

### What it is
The Mud Server adapter provides a basic TCP/IP based telnet server. The server implementation and setup can be compiled and ran on both Windows, OS X and Linux (not tested). Both Windows and OS X have their own server application that uses the server implementation and bootstrap to run.

### What do you need
#### Windows
You will need Visual Studio 2015 and .Net 4.5 to compile the source associated with the `Adapter.MudServer.Windows.sln` solution. Once it has compiled, you can start the MudServer.Windows project and the server will run.

#### OS X
You will need Xamarin Studio and Mono JIT compiler version 4.0.2 to compile the source associated with the `Adapter.MudServer.OSX.sln` solution. Once it has compiled, you can start the MudServer.OSX project and the server will run.
