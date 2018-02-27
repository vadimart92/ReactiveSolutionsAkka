using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;

namespace Remote.Common
{
    public class WorkerActor:ReceiveActor
    {
	    public class GetResult { }
	    public class Result { }
	    public WorkerActor() {
		    Receive<GetResult>(msg => {
			    Console.WriteLine($"Message GetResult received by {Self.Path.Name}");
				Thread.Sleep(TimeSpan.FromSeconds(2));
			    Sender.Tell(new Result());
		    });
	    }
    }
}
