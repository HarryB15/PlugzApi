using System;
using PlugzApi.Models;

namespace PlugzApi.Interfaces
{
    public interface IEmailService
    {
        Error? SendVerificationCodeEmail(string toAddress, int verificationCode);
        Error? SendResetPasswordEmail(string password, string userName, string toAddress);
        Error? SendSupportEmail(SupportReqs req);
    }
}

