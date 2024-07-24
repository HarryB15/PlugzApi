using System;
using System.Net.Mail;
using PlugzApi.Interfaces;
using PlugzApi.Models;

namespace PlugzApi.Services
{
	public class EmailService: IEmailService
	{
        private readonly IConfiguration _configuration;
        private readonly string fromAddress = "hbirchall.dev@outlook.com";
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private string header = @"<!DOCTYPE html>
            <html>
                <head>
                <meta charset='UTF-8'>
                <meta name=""color-scheme"" content=""light"">
                <meta name=""supported-color-schemes"" content=""light"">
                <style>
                    .footer { margin-top: 20px; padding-top: 10px; border-top: 1px solid #ccc; text-align: center; font-size: 12px; color: #999;}
                    .header{ font-size:28px; font-weight: bold; margin: 20px; color: white; text-align: center; }
                    .inner-body { margin: 20px; background: white; padding: 10px; border-radius: 12px; text-align: center; }
                    .small-text { font-size: 12px; color: #ccc; }
                    .code-div {width: 90%; background: #eee; margin: auto; border-radius: 12px; padding: 20px;}
                    .large-text {font-size: 18px; font-weight: 500;}
                    .code-span { margin: 20px; display: block; font-size: 28px; }
                    p{ font-family: system-ui; }
                </style>
                </head>
                <body style='background: linear-gradient(326deg, #a4508b 0%, #9400d3 45%, #7100a9 100%); padding-bottom: 20px;'>
                    <div class='header'><p>Plugz</p></div>
                    <div class='inner-body'>";
        private string footer = "</div></body></html>";
        public Error? SendVerificationCodeEmail(string toAddress, int verificationCode)
        {
            try
            {
                string subject = "Verify Account";
                string body = header + $"<p><span class='large-text'>Welcome</span><br><br>" +
                    $"<br><br>You're almost ready to start using Plugz. Please enter the verification code below on our app.</p>" +
                    $"<div class='code-div'><p><span class='large-text'>Verification code:</span><br>" +
                    $"<span class='code-span'>{verificationCode}</span></p></div>" +
                    $"<p><span class='small-text'>This code will expire in 15 minutes</span><br><br>" +
                    $"Thank you for using our services.<br><br></p>" + footer;

                MailMessage message = new MailMessage(fromAddress, toAddress, subject, body);
                message.IsBodyHtml = true;

                SmtpClient client = new SmtpClient("smtp-mail.outlook.com", 587);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(fromAddress, _configuration["EmailPassword"]);
                client.Send(message);
                return null;
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                return CommonService.Instance.GetUnexpectedErrrorMsg();
            }
        }
        public Error? SendResetPasswordEmail(string password, string userName, string toAddress)
        {
            try
            {
                string subject = "Reset Password";
                string body = header + $"<p><span class='large-text'>Hi {userName}</span><br><br>" +
                    $"<br><br>We have received a request to reset the password for your account. " +
                    $"Your new password can be found below.</p>" +
                    $"<div class='code-div'><p><span class='large-text'>Password:</span><br>" +
                    $"<span class='code-span'>{password}</span></p></div>" +
                    $"<p><span class='small-text'>If you did not submit this request please reply to this email ASAP.</span><br><br>" +
                    $"Thank you for using our services.<br><br></p>" + footer;

                MailMessage message = new MailMessage(fromAddress, toAddress, subject, body);
                message.IsBodyHtml = true;

                SmtpClient client = new SmtpClient("smtp-mail.outlook.com", 587);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(fromAddress, _configuration["EmailPassword"]);
                client.Send(message);
                return null;
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                return CommonService.Instance.GetUnexpectedErrrorMsg();
            }
        }
    }
}

