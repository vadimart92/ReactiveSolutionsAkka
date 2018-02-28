namespace SwitchingBehaviour
{
	using System;
	using Akka.Actor;

	class Program
	{
		static void Main(string[] args) {
			Console.WriteLine("Demo. Switching Behaviour");
			ActorSystem system = ActorSystem.Create("bpmonline");
			var processInstanceActor = system.ActorOf(Props.Create<ProcessInstanceActor>());
			while (true) {
				int elementId = int.Parse(Console.ReadKey().KeyChar.ToString());
				Console.WriteLine();
				processInstanceActor.Tell(new ProcessInstanceActor.Continue(elementId));
			}
		}
	}

	class ProcessInstanceActor : ReceiveActor, IWithUnboundedStash
	{
		public IStash Stash { get; set; }

		#region Mesasges

		public class MsgWithElemenId
		{

			public MsgWithElemenId(int id) {
				ElementId = id;
			}
			public int ElementId { get; private set; }
		}

		public class Continue : MsgWithElemenId
		{

			public Continue(int id)
				: base(id) { }

		}
		public class Completed : MsgWithElemenId
		{

			public Completed(int id)
				: base(id) { }

		}
		

		#endregion

		public ProcessInstanceActor() {
			Become(Idle);
		}

		private void Idle() {
			Receive<Continue>(from => {
				Process.ExecuteAsync(from.ElementId)
					.ContinueWith(task => new Completed(from.ElementId))
					.PipeTo(Self);
				Become(Busy);
			});
		}

		private void Busy() {
			Receive<Continue>(from => {
				Console.WriteLine($"\t\t {from.ElementId} wait please!");
				Stash.Stash();
			});
			Receive<Completed>(completed => {
				Console.WriteLine($"\t\t {completed.ElementId} completed.");
				Stash.UnstashAll();
				Become(Idle);
			});
		}

	}

}
