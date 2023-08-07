using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaloriesTracking.Core;
public class SendGridEmailManager
{
    private readonly ISendGridClient sendGridClient;
    private readonly ILogger<SendGridEmailManager> logger;

    public SendGridEmailManager(ISendGridClient sendGridClient, ILogger<SendGridEmailManager> logger)
    {
        this.sendGridClient = sendGridClient;
        this.logger = logger;
    }

    public async Task SendEmail(string toEmail, string subject, string content)
    {
        var message = new SendGridMessage()
        {
            From = new EmailAddress("lazarst.pn@gmail.com", "CaloriesTracking"),
            Subject = subject,
            HtmlContent = content,
        };

        message.AddTo(new EmailAddress(toEmail));

        // TODO: odraditi whitelistovanje email-a

        var response = await sendGridClient.SendEmailAsync(message);
        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("Email queued successfully");
        }
        else
        {
            logger.LogError("Failed to send email");
        }
    }

    public async Task SendForgotPasswordEmail(string email, string passwordUrl)
    {
        var subject = "CaloriesTracking Password Reset";
        var content =
        $@"<html>
            <head>
                <title>{subject}</title>
            </head>
            <body>
                <p>Hello,</p>
                <p>We received a request to reset your password. Click the link below to reset your password:</p>
                <p><a href='{passwordUrl}'>Click here to reset you password</a></p>
                <p>If you did not request a password reset, please ignore this email.</p>
                <p>Best regards,</p>
                <p>Your CaloriesTracking Team</p>
            </body>
        </html>";
        await SendEmail(email, subject, content);
    }
}
