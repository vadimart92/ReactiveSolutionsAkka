using System;
using Akka.Actor;

namespace HelloWorld
{
	class Program
	{
		static void Main(string[] args) {
			Console.ReadKey(true);
			//single instance per logical app
			ActorSystem system = ActorSystem.Create("hello-world-system");
			//used to instantiate actor instance
			Props props = Props.Create<HelloActor>("john");
			//used to send messages
			IActorRef actor = system.ActorOf(props);
			actor.Tell("world");

			Console.ReadKey(true);
			system.Terminate();
			system.WhenTerminated.Wait();
		}
	}

	internal class HelloActor : UntypedActor
	{
		private readonly string _myName;
		public HelloActor(string myName) {
			_myName = myName;
		}
		protected override void OnReceive(object message) {
			Console.WriteLine($"Hello {message} from {_myName}");
		}
	}

}
