using System;
using Microsoft.Graph.Models;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PlugzApi.Models
{
	public class Keywords: Base
	{
		public int keywordId { get; set; }
		public string keyword { get; set; } = "";
        public bool isDeleted { get; set; }
		public async Task<List<Keywords>> GetKeywords(int listingId)
		{
            List<Keywords> keywords = new List<Keywords>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetKeywords", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listingId;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    Keywords keyword = new Keywords()
                    {
                        keywordId = (int)sdr["KeywordId"],
                        keyword = (string)sdr["Keyword"]
                    };
                    keywords.Add(keyword);
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return keywords;
        }
    }
}

