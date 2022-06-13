using Microsoft.AspNetCore.Mvc;
using MVC_PersonalSite.Models;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using MimeKit; //Added for access to MimeMessage class
using MailKit.Net.Smtp; //Added for access to SmtpClient class


namespace MVC_PersonalSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }      

        [HttpPost]
        public IActionResult Contact(ContactViewModel cvm)
        {
            //check for valid data before processing
            if (!ModelState.IsValid)
            {
                //send them back to the form. Pass the object to the View 
                return View(cvm);
            }

            //create email format
            string message = $"You have received a new email from your site's contact form!<br/><br/>" +
                $"Sender: {cvm.Name}<br/>" +
                $"Email: {cvm.Email}<br/>" +
                $"Subject: {cvm.Subject}<br/>" +
                $"Message: {cvm.Message}";

            //create a MimeMessage object to assist with sending message
            var mm = new MimeMessage();
            
            //assign values to Mime Message
            mm.From.Add(new MailboxAddress("User", _config.GetValue<string>("Credentials:Email:User")));
            mm.To.Add(new MailboxAddress("Personal", _config.GetValue<string>("Credentials:Email:Recipient")));
            mm.Subject = cvm.Subject;
            mm.Body = new TextPart("HTML") { Text = message };
            mm.Priority = MessagePriority.Urgent;
            mm.ReplyTo.Add(new MailboxAddress("Sender", cvm.Email));

            //the using directive will create the SmtpClient object, which is used to send the email
            //once all of the code inside the using directive's scope has been executed, it will 
            //automatically close any open connections and dispose of the object for us.

            using (var client = new SmtpClient())
            {
                //connect to the mail server 
                client.Connect(_config.GetValue<string>("Credentials:Email:Client"));
                client.Authenticate(_config.GetValue<string>("Credentials:Email:User"),
                                    _config.GetValue<string>("Credentials:Email:Password"));

                //Catch exceptions
                try
                {
                    client.Send(mm);
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = $"There was an error processing your request. Please try " +
                        $"again later. <br/><br/>Error Message: ${ex.StackTrace}";

                    return View(cvm);
                }

            }

            //display confirmation on send
            return View("EmailConfirmation", cvm);
        }

    }
}