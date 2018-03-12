using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.Remote;
using Akka.Routing;
using Remote.Common;

namespace Remote.App
{
	using System.IO;

	class Program {
		
		static void Main(string[] args) {
			Console.WriteLine("Demo. Remote. Press key to start.");
			Console.ReadKey(true);
			var system = ActorSystem.Create("remoteDeploy", ConfigurationFactory.ParseString(File.ReadAllText("Remote.App.hocon")));
			var worker = Pool(system);
			var client = system.ActorOf(Props.Create(() => new ClientActor(worker)));
			for (int i = 0; i < 5; i++) {
				client.Tell(new ClientActor.Start());
			}
			Console.ReadKey(true);
		}

		// 1
		private static IActorRef Single(ActorSystem system) {
			var props = Props.Create<WorkerActor>();
			return system.ActorOf(props, "worker");
		}
		
		// 2
		private static IActorRef Group(ActorSystem system) {
			var props = Props.Create<WorkerActor>();
			system.ActorOf(props, "localWorker1");
			system.ActorOf(props, "localWorker2");
			return system.ActorOf(Props.Empty.WithRouter(new RoundRobinGroup("/user/localWorker1",
				"/user/localWorker2")), "group");
		}

		// 3
		private static IActorRef Pool(ActorSystem system) {
			var props = Props.Create<WorkerActor>().WithRouter(new RoundRobinPool(10));
			return system.ActorOf(props, "worker");
		}

		class ClientActor:ReceiveActor
		{
			public class Start { }

			public ClientActor(IActorRef actorRef) {
				Context.Watch(actorRef);
				Context.System.EventStream.Subscribe(Self, typeof(DisassociatedEvent));
				Receive<Start>(msg => {
					actorRef.Tell(new WorkerActor.GetResult());
				});
				Receive<WorkerActor.Result>(result => {
					Console.WriteLine($"\tResult computed by {Sender.Path.Name}.");
				});
				Receive<Terminated>(terminated => {
					Console.WriteLine($"{terminated.ActorRef.Path.Name} terminated");
				});
				Receive<DisassociatedEvent>(disassociated => {
					Console.WriteLine($"Disassociated with {disassociated.RemoteAddress}");
				});
			}
		}

	}
}
