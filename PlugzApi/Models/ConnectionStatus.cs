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
		public byte connectionStatus { get; set; }
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
                connectionStatus = (byte)sdr["ConnectionStatus"];
            }
        }
	}
}

