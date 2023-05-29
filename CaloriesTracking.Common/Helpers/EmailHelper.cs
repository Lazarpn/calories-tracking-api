using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaloriesTracking.Common.Helpers;
public class EmailHelper
{
    public static void SendEmail(string recepient, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse("stojanovic.lazarpn@gmail.com"));
        email.To.Add(MailboxAddress.Parse(recepient));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = body };

        using var smtp = new SmtpClient();
        smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        smtp.Authenticate("stojanovic.lazarpn@gmail.com", "csocvanybnwgssak");
        smtp.Send(email);
        smtp.Disconnect(true);
    }
}
