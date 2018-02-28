using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Remote.Common;

namespace Remote.Node
{
	class Program
	{
		static void Main(string[] args) {
			Console.WriteLine($"Type: {typeof(WorkerActor)} loaded");
			var system = ActorSystem.Create("remoteDeploy", ConfigurationFactory.ParseString(@"
				akka {  
					actor{
						provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
					}
					remote {
						dot-netty.tcp {
							port = 1080
							hostname = localhost
						}
					}
				}"));
			Console.ReadKey(true);
		}
	}
}
