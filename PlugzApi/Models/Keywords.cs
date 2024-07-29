using System;
using Microsoft.Graph.Models;
using PlugzApi.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data;
using System.Data.SqlClient;

namespace PlugzApi.Models
{
	public class Keywords
	{
		public int keywordId { get; set; }
		public string keyword { get; set; } = "";
    }
}

