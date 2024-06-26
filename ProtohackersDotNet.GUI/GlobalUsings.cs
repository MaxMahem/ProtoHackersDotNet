﻿global using System;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.Collections.ObjectModel;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.IO;
global using System.Linq;
global using System.Net;
global using System.Reactive;
global using System.Reactive.Linq;
global using System.Reactive.Threading.Tasks;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using System.Threading;
global using System.Threading.Tasks;

global using DynamicData;

global using Avalonia;
global using Avalonia.Controls;

global using static CommunityToolkit.Diagnostics.ThrowHelper;

global using ProtoHackersDotNet.Helpers;
global using ProtoHackersDotNet.Helpers.ObservableTypes;
global using ProtoHackersDotNet.Servers.Interface;
global using ProtoHackersDotNet.Servers.Interface.Server;
global using ProtoHackersDotNet.Servers.Interface.Server.Events;
global using ProtoHackersDotNet.Servers.Interface.Client;
global using ProtoHackersDotNet.Servers.Interface.Client.Events;
global using ProtoHackersDotNet.Servers.Interface.Exceptions;