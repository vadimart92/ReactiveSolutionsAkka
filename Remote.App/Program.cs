using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.Remote;
using Akka.Routing;
using Remote.Common;

namespace Remote.App
{
	class Program
	{
		
		static void Main(string[] args) {
			Console.ReadKey(true);
			var system = ActorSystem.Create("remoteDeploy", _config);
			var worker = RemoteDeploy(system);
			var client = system.ActorOf(Props.Create(() => new ClientActor(worker)));
			for (int i = 0; i < 5; i++) {
				client.Tell(new ClientActor.Start());
			}
			Console.ReadKey(true);
		}

		private static IActorRef Single(ActorSystem system) {
			var props = Props.Create<WorkerActor>();
			return system.ActorOf(props, "localWorker");
		}

		private static IActorRef Pool(ActorSystem system)
		{
			var props = Props.Create<WorkerActor>().WithRouter(new RoundRobinPool(10));
			return system.ActorOf(props, "localWorker");
		}

		private static IActorRef Group(ActorSystem system) {
			var props = Props.Create<WorkerActor>();
			system.ActorOf(props, "localWorker1");
			system.ActorOf(props, "localWorker2");
			return system.ActorOf(Props.Empty.WithRouter(new RoundRobinGroup("/user/localWorker1",
				"/user/localWorker2")), "group");
		}

		private static IActorRef RemoteDeploy(ActorSystem system) {
			var props = Props.Create<WorkerActor>()
				.WithRouter(new RoundRobinPool(10))
				.WithDeploy(Deploy.None
					.WithScope(new RemoteScope(Address.Parse("akka.tcp://remoteDeploy@localhost:8080"))));
			return system.ActorOf(props, "remoteWorker");
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

		private static Config _config = ConfigurationFactory.ParseString(@"
				akka {  
					loglevel = ""OFF""
					actor{
						provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
					}
					remote {
						dot-netty.tcp {
							port = 0
							hostname = localhost
						}
					}
				}");

	}
}
