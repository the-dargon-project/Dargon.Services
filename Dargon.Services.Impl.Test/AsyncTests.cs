using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Dargon.Ryu;
using NMockito;
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
         var serviceClientFactory = ryu.Get<ServiceClientFactory>();
         var serverServiceClient = serviceClientFactory.Local(kTestServicePort, ClusteringRole.HostOnly);
         serverServiceClient.RegisterService(new ExampleImplementation(), typeof(ExampleInterface));
         
         var clientServiceClient = serviceClientFactory.Local(kTestServicePort, ClusteringRole.GuestOnly);

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

         for (var i = 0; i < 5; i++) {
            stopwatch.Restart();
            Async(() => remoteService.DoNothing()).Wait();
            Trace.WriteLine($"No-op invocation #{i} took {stopwatch.ElapsedMilliseconds} ms!");
         }

         for (var i = 0; i < 5; i++) {
            stopwatch.Restart();
            string iOut = null;
            Async(() => remoteService.OutTest(i, out iOut)).Wait();
            AssertEquals(i + "!", iOut);
            Trace.WriteLine($"Out invocation #{i} took {stopwatch.ElapsedMilliseconds} ms!");
         }

         for (var i = 0; i < 5; i++) {
            stopwatch.Restart();
            string a = "a", b = "b";
            Async(() => remoteService.Swap(ref a, ref b)).Wait();
            AssertEquals("b", a);
            AssertEquals("a", b);
            Trace.WriteLine($"Ref swap invocation #{i} took {stopwatch.ElapsedMilliseconds} ms!");
         }
      }

      [Guid("4EEBA55A-26A3-4143-95F5-4C84708070C7")]
      public interface ExampleInterface {
         string Greet(string name, int age, int incorrectAge, bool trueValue, bool otherTrueValue);
         int Three { get; }
         void DoNothing();
         void OutTest(int a, out string b);
         void Swap<T>(ref T a, ref T b);
      }

      public class IntBox {
         public int value;
      }

      public class ExampleImplementation : ExampleInterface {
         public string Greet(string name, int age, int incorrectAge, bool trueValue, bool otherTrueValue) => $"Hello, {name} who is {age} and not {incorrectAge}... Here's two trues: {trueValue} {otherTrueValue}!";
         public int Three => 3;
         public void DoNothing() { }
         public void OutTest(int a, out string b) => b = a + "!";
         public void Swap<T>(ref T a, ref T b) {
            T temp = a;
            a = b;
            b = temp;
         }
      }
   }
}
