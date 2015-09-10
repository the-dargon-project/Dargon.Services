﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Dargon.Ryu;
using NMockito;
using Xunit;

namespace Dargon.Services {
   public class RefTests : NMockitoInstance {
      private static readonly Guid kWartyAccountId = Guid.NewGuid();
      private static readonly Guid kFredAccountId = Guid.NewGuid();
      private const int kTestServicePort = 30001;

      [Fact]
      public void Run() {
         var ryu = new RyuFactory().Create();
         ryu.Setup();
         var serviceClientFactory = ryu.Get<IServiceClientFactory>();
         var serverClusteringConfiguration = new ClusteringConfiguration(kTestServicePort, 0, ClusteringRoleFlags.HostOnly);
         var serverServiceClient = serviceClientFactory.CreateOrJoin(serverClusteringConfiguration);
         serverServiceClient.RegisterService(new ExampleImplementation(), typeof(ExampleInterface));

         var clientClusteringConfiguration = new ClusteringConfiguration(kTestServicePort, 0, ClusteringRoleFlags.GuestOnly);
         var clientServiceClient = serviceClientFactory.CreateOrJoin(clientClusteringConfiguration);

         var remoteService = clientServiceClient.GetService<ExampleInterface>();

         const string kValueA = "A";
         const string kValueB = "B";
         string a = kValueA, b = kValueB;

         var sw = new Stopwatch();
         for (var i = 0; i < 10; i++) {
            sw.Restart();

            remoteService.Swap(ref a, ref b);

            AssertEquals(kValueB, a);
            AssertEquals(kValueA, b);

            remoteService.Swap(ref a, ref b);

            AssertEquals(kValueA, a);
            AssertEquals(kValueB, b);

            Debug.WriteLine("Two swaps performed in: " + sw.ElapsedMilliseconds);
         }
      }

      [Guid("2257424F-C403-4CA2-A670-8F1FD896F2FB")]
      public interface ExampleInterface {
         void Swap(ref string a, ref string b);
      }

      public class ExampleImplementation : ExampleInterface {
         public void Swap(ref string a, ref string b) {
            var temp = a;
            a = b;
            b = temp;
         }
      }
   }
}