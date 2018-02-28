using System;
using Akka.Actor;

namespace HelloWorld
{
	class Program
	{
		static void Main(string[] args) {
			Console.WriteLine("Demo. Hello world");
			Console.ReadKey(true);
			//ActorSystem is single instance per logical app
			ActorSystem system = ActorSystem.Create("hello-world-system");
			//Props used to instantiate actor instance
			Props props = Props.Create<HelloActor>("john");
			//IActorRef used to send messages
			IActorRef actor = system.ActorOf(props);
			//tell is main way to comunicate actor, there is an Ask<T> 
			// akka.net supports "at most once" delivery semantics, message order between two actors 
			// preserved (akka specific implementation)
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
