﻿global using System;
global using System.Buffers;
global using System.Buffers.Binary;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.IO;
global using System.IO.Pipelines;
global using System.Linq;
global using System.Net;
global using System.Net.Sockets;
global using System.Reactive.Linq;
global using System.Reactive.Subjects;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using System.Threading;
global using System.Threading.Tasks;

global using CommunityToolkit.Diagnostics;
global using static CommunityToolkit.Diagnostics.ThrowHelper;

global using ByteSizeLib;

global using ProtoHackersDotNet.Helpers;

global using ProtoHackersDotNet.Servers.Helpers;
global using ProtoHackersDotNet.Servers.TcpServer;
global using ProtoHackersDotNet.Servers.Interface;
global using ProtoHackersDotNet.Servers.Interface.Client;
global using ProtoHackersDotNet.Servers.Interface.Client.Events;
global using ProtoHackersDotNet.Servers.Interface.Client.Messages;
global using ProtoHackersDotNet.Servers.Interface.Exceptions;
global using ProtoHackersDotNet.Servers.Interface.Server;
global using ProtoHackersDotNet.Servers.Interface.Server.Events;
