using System;
using PlugzApi.Models;

namespace PlugzApi.Interfaces
{
    public interface IEmailService
    {
        Error? SendVerificationCodeEmail(string toAddress, int verificationCode);
    }
}

