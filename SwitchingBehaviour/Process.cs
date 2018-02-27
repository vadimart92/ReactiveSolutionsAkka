namespace SwitchingBehaviour
{
	using System;
	using System.Threading.Tasks;

	class Process {

		public static async Task ExecuteAsync(int elementId) {
			Console.WriteLine($"Executing element: {elementId}");
			await Task.Delay(TimeSpan.FromSeconds(2));
		}

	}
}