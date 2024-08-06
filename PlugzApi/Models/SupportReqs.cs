using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Graph.Models.TermStore;
using PlugzApi.Services;

namespace PlugzApi.Models
{
	public class SupportReqs : Base
	{
        public int supportRequestId { get; set; }
        public int supportReqTypeId { get; set; }
        public int supportReqStatusId { get; set; }
        public string supportReqType { get; set; } = "";
        public string supportReqStatus { get; set; } = "";
        public string details { get; set; } = "";
        public string reference { get; set; } = "";
        public int? extId { get; set; }
        public DateTime sentDatetime { get; set; }
        public DateTime? completedDatetime { get; set; }

        public async Task<List<SupportReqs>> GetUsersSupportRequests()
        {
            List<SupportReqs> reqs = new List<SupportReqs>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersSupportRequests", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    reqs.Add(new SupportReqs()
                    {
                        supportRequestId = (int)sdr["SupportRequestId"],
                        supportReqTypeId = (int)sdr["SupportReqTypeId"],
                        supportReqType = (string)sdr["SupportReqType"],
                        supportReqStatusId = (int)sdr["SupportReqStatusId"],
                        supportReqStatus = (string)sdr["SupportReqStatus"],
                        details = (string)sdr["Details"],
                        reference = (string)sdr["Reference"],
                        extId = (sdr["ExtId"] != DBNull.Value) ? (int)sdr["ExtId"] : null,
                        sentDatetime = (DateTime)sdr["SentDatetime"],
                        completedDatetime = (sdr["CompletedDatetime"] != DBNull.Value) ? (DateTime)sdr["CompletedDatetime"] : null
                    });
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return reqs;
        }
        public async Task InsSupportRequests()
        {
            try
            {
                extId = (supportReqTypeId == 1 || supportReqTypeId == 2) ? extId : null;
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("InsSupportRequests", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@supportReqTypeId", SqlDbType.Int).Value = supportReqTypeId;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@details", SqlDbType.VarChar).Value = details;
                cmd.Parameters.Add("@extId", SqlDbType.Int).Value = extId;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    supportRequestId = (int)sdr["SupportRequestId"];
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
    }
}