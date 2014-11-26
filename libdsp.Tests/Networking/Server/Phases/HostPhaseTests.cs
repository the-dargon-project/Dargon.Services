using System;
using System.IO;
using System.Net.Sockets;
using Dargon.PortableObjects;
using Dargon.Services.Networking.PortableObjects;
using Dargon.Services.Networking.Server.Sessions;
using ItzWarty;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NMockito;
using Xunit;

namespace Dargon.Services.Networking.Server.Phases {
   public class HostPhaseTests : NMockitoInstance {
      private readonly HostPhase testObj;

      [Mock] private readonly IThreadingProxy threadingProxy = null;
      [Mock] private readonly INetworkingProxy networkingProxy = null;
      [Mock] private readonly IPofSerializer pofSerializer = null;
      [Mock] private readonly IHostSessionFactory hostSessionFactory = null;
      [Mock] private readonly IConnectorContext connectorContext = null;
      [Mock] private readonly IListenerSocket listenerSocket = null;
      [Mock] private readonly ICancellationTokenSource cancellationTokenSource = null;

      public HostPhaseTests() {
         When(threadingProxy.CreateCancellationTokenSource()).ThenReturn(cancellationTokenSource);

         testObj = new HostPhase(threadingProxy, networkingProxy, pofSerializer, hostSessionFactory, connectorContext, listenerSocket);

         Verify(threadingProxy).CreateCancellationTokenSource();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void InitializeCreatesListenerThread() {
         var thread = CreateMock<IThread>();
         var optionsCaptor = new ArgumentCaptor<ThreadCreationOptions>();
         When(threadingProxy.CreateThread(Eq<ThreadEntryPoint>(testObj.ListenerThreadEntryPoint), optionsCaptor.GetParameter())).ThenReturn(thread);
         testObj.Initialize();
         Verify(threadingProxy).CreateThread(testObj.ListenerThreadEntryPoint, optionsCaptor.Value);
         Verify(thread).Start();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void ListenerThreadEntryPointTest() {
         var client1 = CreateMock<IConnectedSocket>();
         var client2 = CreateMock<IConnectedSocket>();
         var thread1 = CreateMock<IThread>();
         var thread2 = CreateMock<IThread>();

         When(cancellationTokenSource.IsCancellationRequested).ThenReturn(false, false, true);
         When(listenerSocket.Accept()).ThenReturn(client1, client2);
         When(threadingProxy.CreateThread(Any<ThreadEntryPoint>(), Any<ThreadCreationOptions>())).ThenReturn(thread1, thread2);

         testObj.ListenerThreadEntryPoint();

         Verify(cancellationTokenSource, Times(3)).IsCancellationRequested.Wrap();
         Verify(listenerSocket, Times(2)).Accept();
         Verify(threadingProxy).CreateThread(Any<ThreadEntryPoint>(), Any<ThreadCreationOptions>(x => x.IsBackground));
         Verify(thread1).Start();
         Verify(thread2).Start();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void SessionThreadEntryPointSocketExceptionIsNotFatal() {
         var socket = CreateMock<IConnectedSocket>();
         When(socket.GetReader()).ThenThrow(new SocketException());
         testObj.SessionThreadEntryPoint(socket);
         Verify(socket).GetReader();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void SessionThreadEntryPointExceptionIsNotFatal() {
         var socket = CreateMock<IConnectedSocket>();
         When(socket.GetReader()).ThenThrow(new Exception());
         testObj.SessionThreadEntryPoint(socket);
         Verify(socket).GetReader();
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void SessionThreadEntryPointClientRouteTest() {
         var socket = CreateMock<IConnectedSocket>();
         var reader = new BinaryReader(new MemoryStream());
         var writer = new BinaryWriter(new MemoryStream());
         var clientSession = CreateMock<IClientSession>();
         var handshake = new X2SHandshake(Role.Client);

         When(socket.GetReader()).ThenReturn(reader);
         When(socket.GetWriter()).ThenReturn(writer);
         When(pofSerializer.Deserialize<X2SHandshake>(reader)).ThenReturn(handshake);
         When(hostSessionFactory.CreateClientSession(reader, writer)).ThenReturn(clientSession);

         testObj.SessionThreadEntryPoint(socket);

         Verify(socket).GetReader();
         Verify(socket).GetWriter();
         Verify(pofSerializer).Deserialize<X2SHandshake>(reader);
         Verify(hostSessionFactory).CreateClientSession(reader, writer);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void SessionThreadEntryPointGuestRouteTest() {
         var socket = CreateMock<IConnectedSocket>();
         var reader = new BinaryReader(new MemoryStream());
         var writer = new BinaryWriter(new MemoryStream());
         var guestSession = CreateMock<IGuestSession>();
         var handshake = new X2SHandshake(Role.Guest);

         When(socket.GetReader()).ThenReturn(reader);
         When(socket.GetWriter()).ThenReturn(writer);
         When(pofSerializer.Deserialize<X2SHandshake>(reader)).ThenReturn(handshake);
         When(hostSessionFactory.CreateGuestSession(reader, writer)).ThenReturn(guestSession);

         testObj.SessionThreadEntryPoint(socket);

         Verify(socket).GetReader();
         Verify(socket).GetWriter();
         Verify(pofSerializer).Deserialize<X2SHandshake>(reader);
         Verify(hostSessionFactory).CreateGuestSession(reader, writer);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void SessionThreadEntryPointInvalidHandshakeRouteTest() {
         var socket = CreateMock<IConnectedSocket>();
         var reader = new BinaryReader(new MemoryStream());
         var writer = new BinaryWriter(new MemoryStream());
         var handshake = new X2SHandshake(Role.Undefined);

         When(socket.GetReader()).ThenReturn(reader);
         When(socket.GetWriter()).ThenReturn(writer);
         When(pofSerializer.Deserialize<X2SHandshake>(reader)).ThenReturn(handshake);

         testObj.SessionThreadEntryPoint(socket);

         Verify(socket).GetReader();
         Verify(pofSerializer).Deserialize<X2SHandshake>(reader);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void DisposeTest() {
         testObj.Dispose();
         Verify(cancellationTokenSource).Dispose();
         Verify(listenerSocket).Dispose();
         VerifyNoMoreInteractions();
      }
   }
}
