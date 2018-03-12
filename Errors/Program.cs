using System;
using System.Linq;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;

namespace Errors
{
	class Program
	{
		static void Main(string[] args) {
			Console.WriteLine("Demo. Errors");
			ActorSystem system = ActorSystem.Create("hello-world-system", ConfigurationFactory.ParseString("akka.loglevel = \"OFF\""));
			var actorToSupervise = Props.Create<DivideActor>();
			IActorRef actor = system.ActorOf(Props.Create<Supervisor>(actorToSupervise));
			system.EventStream.Subscribe(new StandardOutLogger(), typeof(DeadLetter));
			while (true) {
				actor.Tell(Console.ReadLine());
			}
		}
	}

	internal class DivideActor : UntypedActor
	{

		private int _state;

		protected override void OnReceive(object message) {
			var expression = message as string;
			if (expression == "s") {
				Console.WriteLine($"State: {_state}");
				return;
			}
			if (string.IsNullOrWhiteSpace(expression)) {
				throw new NullReferenceException();
			}
			var numbers = expression.Select(c => int.Parse(c.ToString())).ToArray();
			var result = numbers[0] / numbers[1];
			Console.WriteLine($"Result: {result}");
			_state++;
		}

		////1
		//public override void AroundPreRestart(Exception cause, object message) {
		//	base.AroundPreRestart(cause, message);
		//	Console.WriteLine($"Error: {cause.Message} on message with type {message.GetType().Name}");
		//}

	}

	internal class Supervisor : ReceiveActor
	{
		public Supervisor(Props childProps ) {

			var actorToSupervise = Context.ActorOf(childProps);

			//3 - Deathwatch
			Context.Watch(actorToSupervise);
			Receive<Terminated>(terminated => {
				if (terminated.ActorRef.Equals(actorToSupervise)) {
					actorToSupervise = ActorRefs.Nobody;
					Console.WriteLine("Actor died");
				}
			});

			Receive<object>(msg => {
				//3
				if (actorToSupervise.IsNobody()) {
					Console.WriteLine("too many errors");
					return;
				}
				actorToSupervise.Forward(msg);
			});
		}

		 //2 
		protected override SupervisorStrategy SupervisorStrategy() {
			return new OneForOneStrategy(3, TimeSpan.FromSeconds(2), new LocalOnlyDecider(exception => {
				switch (exception) {
					case DivideByZeroException _:
					case IndexOutOfRangeException _:
						Console.WriteLine($"resuming on: {exception.Message}");
						return Directive.Stop;
					case NullReferenceException _:
						Console.WriteLine($"restarting on: {exception.Message}");
						return Directive.Restart;
					default:
						return Directive.Stop;
				}
			}));
		}

	}

}
