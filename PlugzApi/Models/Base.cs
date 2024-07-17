using System;
using System.Data.SqlClient;
using System.Text.Json.Serialization;

namespace PlugzApi.Models
{
	public class Base
	{
        public SqlConnection? con = null;
        public SqlCommand? cmd = null;
        public SqlDataReader? sdr = null;
        [JsonIgnore]
        public Error? error { get; set; }
        public int userId { get; set; }
    }
}

