using Dargon.Ryu;
using NMockito;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;
using static Dargon.Services.AsyncStatics;

namespace Dargon.Services {
   public class AsyncTests : NMockitoInstance {
      private const int kTestServicePort = 30000;

      [Fact]
      public void Run() {
         const string kExpectedResult = "Hello, Fred who is 8 and not 27... Here's two trues: True True!";

         var ryu = new RyuFactory().Create();
         ryu.Setup();
         var serviceClientFactory = ryu.Get<IServiceClientFactory>();
         var serverClusteringConfiguration = new ClusteringConfiguration(kTestServicePort, 0, ClusteringRoleFlags.HostOnly);
         var serverServiceClient = serviceClientFactory.CreateOrJoin(serverClusteringConfiguration);
         serverServiceClient.RegisterService(new ExampleImplementation(), typeof(ExampleInterface));

         var clientClusteringConfiguration = new ClusteringConfiguration(kTestServicePort, 0, ClusteringRoleFlags.GuestOnly);
         var clientServiceClient = serviceClientFactory.CreateOrJoin(clientClusteringConfiguration);

         var remoteService = clientServiceClient.GetService<ExampleInterface>();
         var echoName = "Fred";
         var incorrectAgesIndex = new int[] { 23, 0 };
         var incorrectAges = new int[] { 27 };
         var one = 1;
         var oneTwoThreeBoxBox = new Tuple<IntBox>(new IntBox());
         int twoThreeFour = -1;
         var stopwatch = new Stopwatch();
         for (var i = 0; i < 5; i++) {
            stopwatch.Restart();
            var easyTask = Async(() => remoteService.Greet("Fred", 8, 27, true, true));
            var easyResult = easyTask.Result;
            Trace.WriteLine($"Easy async invocation #{i} took {stopwatch.ElapsedMilliseconds} ms!");
            AssertEquals(kExpectedResult, easyResult);
         }

         for (var i = 0; i < 5; i++) {
            stopwatch.Restart();
            var doubleServiceTask = Async(() => remoteService.Greet(echoName, (int)Math.Pow(one + int.Parse("1"), remoteService.Three), 27, true, true));
            var doubleServiceResult = doubleServiceTask.Result;
            Trace.WriteLine($"Double service async invocation ${i} took {stopwatch.ElapsedMilliseconds} ms!");
            AssertEquals(kExpectedResult, doubleServiceResult);
         }

         for (var i = 0; i < 5; i++) {
            stopwatch.Restart();
            var mediumTask = Async(() => remoteService.Greet(echoName, (int)Math.Pow(one + int.Parse("1"), 3), 27, int.TryParse("123", out oneTwoThreeBoxBox.Item1.value), int.TryParse("234", out twoThreeFour)));
            var mediumResult = mediumTask.Result;
            Trace.WriteLine($"Medium async invocation #{i} took {stopwatch.ElapsedMilliseconds} ms!");
            AssertEquals(kExpectedResult, mediumResult);
            AssertEquals(123, oneTwoThreeBoxBox.Item1.value);
            AssertEquals(234, twoThreeFour);
         }

         for (var i = 0; i < 5; i++) {
            stopwatch.Restart();
            var hardTask = Async(() => remoteService.Greet(echoName, (int)Math.Pow(one + int.Parse("1"), remoteService.Three), incorrectAges[incorrectAgesIndex[1]], int.TryParse("123", out oneTwoThreeBoxBox.Item1.value), int.TryParse("234", out twoThreeFour)));
            var hardResult = hardTask.Result;
            Trace.WriteLine($"Hard async invocation #{i} took {stopwatch.ElapsedMilliseconds} ms!");
            AssertEquals(kExpectedResult, hardResult);
            AssertEquals(123, oneTwoThreeBoxBox.Item1.value);
            AssertEquals(234, twoThreeFour);
         }
      }

      [Guid("4EEBA55A-26A3-4143-95F5-4C84708070C7")]
      public interface ExampleInterface {
         string Greet(string name, int age, int incorrectAge, bool trueValue, bool otherTrueValue);
         int Three { get; }
      }

      public class IntBox {
         public int value;
      }

      public class ExampleImplementation : ExampleInterface {
         public string Greet(string name, int age, int incorrectAge, bool trueValue, bool otherTrueValue) => $"Hello, {name} who is {age} and not {incorrectAge}... Here's two trues: {trueValue} {otherTrueValue}!";
         public int Three => 3;
      }
   }
}
