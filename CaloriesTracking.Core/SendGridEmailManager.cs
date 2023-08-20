using CaloriesTracking.Common.Enums;
using CaloriesTracking.Common.Exceptions;
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
            throw new ValidationException(ErrorCode.EmailNotSent);
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

    public async Task SendVerificationEmail(string email, string verificationCode)
    {
        var subject = "CaloriesTracking Email Confirmation";
        var content =
        $@"<html>
            <head>
                <title>Hi! Welcome to CaloriesTracking</title>
            </head>
            <body>
                <p>
                Thank you for registering with us.To finish setting up your account,
                please confirm your email address by entering the code below into
                the verification window on the StudyStream app. This code expires in 15 minutes.
                If you didn't request this code, simply ignore this email
                </p>
                <p>Your verification code is:</p>
                <p>{verificationCode}</p>
                <p>Best regards,</p>
                <p>Your CaloriesTracking Team</p>
            </body>
        </html>";
        await SendEmail(email, subject, content);
    }
}
