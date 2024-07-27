using System;
using Microsoft.Graph.Models;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;

namespace PlugzApi.Models
{
	public class ConnectionStatus: Base
	{
		public int contactUserId { get; set; }
        public int contactId { get; set; }
        public int requestId { get; set; }
		public int connectionStatus { get; set; }
		public async Task GetConnectionStatus()
		{
            con = await CommonService.Instance.Open();
            cmd = new SqlCommand("GetConnectionStatus", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@contactUserId", SqlDbType.Int).Value = contactUserId;
            sdr = await cmd.ExecuteReaderAsync();
            if (sdr.Read())
            {
                contactId = (int)sdr["ContactId"];
                requestId = (int)sdr["RequestId"];
                connectionStatus = (int)sdr["ConnectionStatus"];
            }
        }
	}
}

