namespace SwitchingBehaviour
{
	using System;
	using System.Threading.Tasks;
	using Akka.Actor;
	using Akka.Event;

	class Program
	{
		static void Main(string[] args) {
			ActorSystem system = ActorSystem.Create("bpmonline");
			var processActor = system.ActorOf(Props.Create<ProcessInstanceActor>());
			while (true) {
				int elementId = int.Parse(Console.ReadKey().KeyChar.ToString());
				Console.WriteLine();
				processActor.Tell(new ProcessInstanceActor.ContinueFrom(elementId));
			}
		}
	}

	class ProcessInstanceActor : ReceiveActor, IWithUnboundedStash
	{
		public IStash Stash { get; set; }

		#region Mesasges

		public class ContinueFrom
		{

			public ContinueFrom(int id) {
				ElementId = id;
			}
			public int ElementId { get; private set; }
		}

		private class Completed : ContinueFrom {

			public Completed(int id)
				: base(id) { }

		}
		

		#endregion

		public ProcessInstanceActor() {
			Become(Ready);
		}

		private void Ready() {
			Receive<ContinueFrom>(from => {
				Process.ExecuteAsync(from.ElementId).ContinueWith(task => new Completed(from.ElementId)).PipeTo(Self);
				Become(Working);
			});
		}

		private void Working() {
			Receive<Completed>(completed => {
				Console.WriteLine($"\t {completed.ElementId} completed.");
				Become(Ready);
				Stash.UnstashAll();
			});
			Receive<ContinueFrom>(from => {
				Console.WriteLine($"\t {from.ElementId} wait please!");
				Stash.Stash();
			});
		}

	}


	class Process {

		public static async Task ExecuteAsync(int elementId) {
			Console.WriteLine($"Executing element: {elementId}");
			await Task.Delay(TimeSpan.FromSeconds(2));
		}

	}
}
