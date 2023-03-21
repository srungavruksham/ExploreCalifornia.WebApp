using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace ExploreCalifornia.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        [HttpPost]
        [Route("Book")]
        public IActionResult Book()
        {
            var tourname = Request.Form["tourname"];
            var name = Request.Form["name"];
            var email= Request.Form["email"];
            var needsTransport = Request.Form["transport"] == "on";

            // Send messages here...
            // connection to the RabbitMQ instance

            var factory = new ConnectionFactory();  // to create this we create an instance of ConnectionFactory Class
            factory.Uri = new Uri("amqp://guest:guest@localhost:5672");  //  The factory needs a URI. This is the uniform resource identifier. It tells the ConnectionFactory where our instance is located
            var connection = factory.CreateConnection(); // From this factory, we can now request a connection

            //  from the connection, we can request a channel by calling the connection.CreateModel method.
            //  Remember that a channel is a virtual connection.
            //  A client can use a single connection for multiple independent threads of messages
            var channel = connection.CreateModel();

            // create an exchange, call the channel.ExchangeDeclare method
            //  The first parameter is our exchange name. I'll call mine webappExchange.
            //  Then we need to define the exchange type, and we'll use a fanout for now, and finally, we need to decide if we want a durable exchange or not
            channel.ExchangeDeclare("webappExchange", ExchangeType.Fanout, true);

            var message = $"{tourname};{name};{email}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(message);

            // we'll call the channel.BasicPublish method to actually publish our message.
            // We need to pass in the exchange name, which was webappExchange, the routing key, which we can leave empty for now, because we used a fanout exchange,
            // then the basic properties, which we can leave empty as well, and finally, our raw bytes
            channel.BasicPublish("webappExchange", "", null, bytes);

            // declare resources here, handle consumed events, etc

            channel.Close();
            connection.Close();

            return Redirect($"/BookingConfirmed?tourname={tourname}&name={name}&email={email}");
        }

        [HttpPost]
        [Route("Cancel")]
        public IActionResult Cancel()
        {
            var tourname = Request.Form["tourname"];
            var name = Request.Form["name"];
            var email = Request.Form["email"];
            var cancelReason = Request.Form["reason"];

            // Send cancel message here

            return Redirect($"/BookingCanceled?tourname={tourname}&name={name}");
        }
    }
}