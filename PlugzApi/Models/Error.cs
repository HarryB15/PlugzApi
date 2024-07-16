using System;
using Microsoft.AspNetCore.Mvc;

namespace PlugzApi.Models
{
	public class Error
	{
        public string? errorMsg { get; set; }
        public int errorCode { get; set; }
    }
}

