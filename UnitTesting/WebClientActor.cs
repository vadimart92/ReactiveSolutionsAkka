using System.Net;
using Akka.Actor;
using Akka.TestKit.NUnit3;
using FluentAssertions;
using NUnit.Framework;

namespace UnitTesting
{
    public class WebClientActor : ReceiveActor
    {
	    public class DownloadRequest {
		    public string Address { get; set; }
	    }
	    public class DownloadResult {
		    public string Content { get; set; }
	    }

	    public WebClientActor() {
		    IActorRef sender = ActorRefs.Nobody;
		    Receive<DownloadRequest>(request => {
			    sender = Sender;
			    var client = new WebClient();
				client.DownloadStringTaskAsync(request.Address).PipeTo(Self);
			});
		    Receive<string>(content => sender.Tell(new DownloadResult {
			    Content = content
		    }));
	    }
    }

	[TestFixture]
	public class WebClientActorTests : TestKit
	{
		[Test]
		public void DownloadRequest() {
			var webClient = Sys.ActorOf(Props.Create<WebClientActor>());
			webClient.Tell(new WebClientActor.DownloadRequest { Address = "http://www.google.com" });
			var result = ExpectMsg<WebClientActor.DownloadResult>();
			result.Content.Should().NotBeNullOrWhiteSpace();
		}
	}
}
