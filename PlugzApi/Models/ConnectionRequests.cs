using System;
using Microsoft.Graph.Models;
using PlugzApi.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data;
using System.Data.SqlClient;

namespace PlugzApi.Models
{
	public class ConnectionRequests: Base
	{
		public int requestId { get; set; }
        public int connectionUserId { get; set; }
        public DateTime requestDatetime { get; set; }
        public bool accepted { get; set; }
        public string userName { get; set; } = "";

        public async Task InsConnectionRequests()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("InsConnectionRequests", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@connectionUserId", SqlDbType.Int).Value = connectionUserId;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
        }
        public async Task ConnectionRequestResponse()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("ConnectionRequestResponse", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@requestId", SqlDbType.Int).Value = requestId;
                cmd.Parameters.Add("@accepted", SqlDbType.Bit).Value = accepted;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
        }
        public async Task<List<ConnectionRequests>> GetUserConnectionRequests()
        {
            List<ConnectionRequests> requests = new List<ConnectionRequests>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUserConnectionRequests", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    ConnectionRequests request = new ConnectionRequests()
                    {
                        requestId = (int)sdr["requestId"],
                        userId = (int)sdr["UserId"],
                        connectionUserId = userId,
                        requestDatetime = (DateTime)sdr["RequestDatetime"],
                        userName = (string)sdr["UserName"]
                    };
                    requests.Add(request);
                }
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
            return requests;
        }
        public async Task DeleteConnectionRequest()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("DeleteConnectionRequest", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@requestId", SqlDbType.Int).Value = requestId;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
        }
    }
}