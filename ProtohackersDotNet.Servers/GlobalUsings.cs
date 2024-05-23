global using System;
global using System.Buffers;
global using System.Buffers.Binary;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.Diagnostics.CodeAnalysis;
global using System.IO;
global using System.IO.Pipelines;
global using System.Linq;
global using System.Net;
global using System.Net.Sockets;
global using System.Threading;
global using System.Threading.Tasks;

global using CommunityToolkit.Diagnostics;

global using ByteSizeLib;

global using ProtoHackersDotNet.Servers.Helpers;
global using ProtoHackersDotNet.Servers.TcpServer;
global using ProtoHackersDotNet.Servers.Interfaces.Client;
global using ProtoHackersDotNet.Servers.Interfaces.Server;
