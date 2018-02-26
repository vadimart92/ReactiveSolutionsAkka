using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace Errors
{
	class Program
	{
		static void Main(string[] args) {
			ActorSystem system = ActorSystem.Create("hello-world-system", ConfigurationFactory.ParseString("akka.loglevel = \"OFF\""));
			var actorToSupervise = Props.Create<DivideActor>();
			IActorRef actor = system.ActorOf(Props.Create<Superisor>(actorToSupervise));
			while (true) {
				string expression = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(expression)) {
					actor.Tell(new NullReferenceException());
				} else {
					actor.Tell(expression);
				}
			}
		}
	}

	internal class DivideActor : UntypedActor
	{

		private int _state;

		protected override void OnReceive(object message) {
			switch (message) {
				case string expression: {
					if (expression == "s") {
						Console.WriteLine($"State: {_state}");
						return;
					}
					var numbers = expression.Select(c => int.Parse(c.ToString())).ToArray();
					var result = numbers[0] / numbers[1];
					Console.WriteLine($"Result: {result}");
					_state++;
					break;
				}
				case Exception e:
					throw e;
			}
		}

		public override void AroundPreRestart(Exception cause, object message) {
			base.AroundPreRestart(cause, message);
			Console.WriteLine($"Error: {cause.Message}");
		}

	}

	internal class Superisor : ReceiveActor
	{
		public Superisor(Props childProps ) {

			var actorToSupervise = Context.ActorOf(childProps);

			Context.Watch(actorToSupervise);
			Receive<Terminated>(terminated => {
				if (terminated.ActorRef.Equals(actorToSupervise)) {
					actorToSupervise = ActorRefs.Nobody;
					Console.WriteLine("Actor died");
				}
			});

			Receive<object>(msg => {
				if (actorToSupervise.IsNobody()) {
					Console.WriteLine("too many errors");
				} else {
					actorToSupervise.Forward(msg);
				}
			});
		}

		protected override SupervisorStrategy SupervisorStrategy() {
			return new OneForOneStrategy(3, TimeSpan.FromSeconds(2), new LocalOnlyDecider(exception => {
				switch (exception) {
					case DivideByZeroException _:
					case IndexOutOfRangeException _:
						return Directive.Resume;
					case NullReferenceException _:
						return Directive.Restart;
					default:
						return Directive.Stop;
				}
			}));
		}
	}

}
