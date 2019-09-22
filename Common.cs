using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Security;



namespace DataAccessLayer
{
    public class Common
    {
        SqlConnection con;
        SqlCommand sqlcmd;
        DataSet dsgetdata;
        string strcon = ConfigurationManager.AppSettings["108"].ToString();

        public DataSet registration(string fundcode)
        {
            DataSet dsgetdata = new DataSet();
            sqlcmd = new SqlCommand();
            con = new SqlConnection(TamperProofStringDecode(strcon, "KBCONSTR"));
            SqlDataAdapter dagetdata = new SqlDataAdapter("AXIS_S2S_IMPS_REGISTER_Webservice", con);
            dagetdata.SelectCommand.CommandType = CommandType.StoredProcedure;
            dagetdata.SelectCommand.Parameters.Add("@fund", SqlDbType.VarChar, 200).Value = fundcode.Trim();
            dagetdata.SelectCommand.CommandTimeout = 600;
            dagetdata.Fill(dsgetdata);
            return dsgetdata;
        }

        public DataSet getRegistrators(string fundcode)
        {
            DataSet dsgetdata = new DataSet();
            sqlcmd = new SqlCommand();
            con = new SqlConnection(TamperProofStringDecode(strcon, "KBCONSTR"));
            SqlDataAdapter dagetdata = new SqlDataAdapter("AXIS_S2S_IMPS_REGISTER_Webservice", con);
            dagetdata.SelectCommand.CommandType = CommandType.StoredProcedure;
            dagetdata.SelectCommand.Parameters.Add("@fund", SqlDbType.VarChar, 200).Value = fundcode.Trim();
            dagetdata.SelectCommand.CommandTimeout = 600;
            dagetdata.Fill(dsgetdata);
            return dsgetdata;
        }
        public DataSet getInserlogrequest(string URL)
        {
            string hostName = Dns.GetHostName(); // Retrive the Name of HOST
            // Get the IP
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            DataSet ds = new DataSet();
            List<SqlParameter> plist = new List<SqlParameter>();
            SqlParameter p;
            p = new SqlParameter("@i_reqparam", SqlDbType.VarChar, 8000);
            p.Value = URL;
            plist.Add(p);
            //p = new SqlParameter("@i_Method", SqlDbType.VarChar, 50);
            //p.Value = Method;
            //plist.Add(p);
            ds = DataAccessLayer.Common.ExecuteDataSet("Karvymfs_CommonServices_InstaResplog", plist, TamperProofStringDecode(strcon, "KBCONSTR"));
            return ds;
        }

        public void updatelogrequest(int id, string request)
        {
            DataSet ds = new DataSet();
            List<SqlParameter> plist = new List<SqlParameter>();
            SqlParameter p;
            p = new SqlParameter("@i_ID", SqlDbType.Int, 20);
            p.Value = id;
            plist.Add(p);
            p = new SqlParameter("@i_response", SqlDbType.VarChar, 8000);
            p.Value = request;
            plist.Add(p);
            ds = DataAccessLayer.Common.ExecuteDataSet("KarvymfsCommonServices_insta_UpdateResplog", plist, TamperProofStringDecode(strcon, "KBCONSTR"));
        }
        public static DataSet ExecuteDataSet(String SPName, List<SqlParameter> paramlist, String DBConnectionString)
        {
            //CreateErrorLogSessions("DataAccessLayer.Common.ExecuteDataSet");

            DataSet ds = null;
            try
            {
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = DBConnectionString;
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = SPName;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Connection = con;
                        if (paramlist != null && paramlist.Count > 0)
                        {
                            foreach (SqlParameter p in paramlist)
                            {
                                cmd.Parameters.Add(p);
                            }
                        }
                        using (SqlDataAdapter da = new SqlDataAdapter())
                        {
                            da.SelectCommand = cmd;
                            ds = new DataSet();
                            da.Fill(ds);
                            ds.DataSetName = "Dataset1";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw ex;
                //throw ex;
            }
            catch (FaultException ex)
            {
                throw ex;
                //throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
                //throw ex;
            }

            return ds;


        }

        #region TamperProof

        public string TamperProofStringEncode(string value, string key)
        {
            System.Security.Cryptography.MACTripleDES mac3des = new System.Security.Cryptography.MACTripleDES();
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            mac3des.Key = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key));
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value)) + "-" + Convert.ToBase64String(mac3des.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value)));
        }

        public string TamperProofStringDecode(string value, string key)
        {
            string dataValue = "";
            string calcHash = "";
            string storedHash = "";
            System.Security.Cryptography.MACTripleDES mac3des = new System.Security.Cryptography.MACTripleDES();
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            mac3des.Key = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key));

            try
            {
                dataValue = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value.Split('-')[0]));
                storedHash = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value.Split('-')[1]));
                calcHash = System.Text.Encoding.UTF8.GetString(mac3des.ComputeHash(System.Text.Encoding.UTF8.GetBytes(dataValue)));
                if ((storedHash != calcHash))
                {
                    throw new ArgumentException("Hash value does not match");
                    // This error is immediately caught below
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid TamperProofString");
            }
            return dataValue;
        }
        #endregion


        public void InsertResponse(string statusCode, string StatusMessage, string Ihno, string BankReferenceNumber)
        {
            using (SqlConnection con = new SqlConnection(TamperProofStringDecode(strcon, "KBCONSTR")))
            {
                SqlCommand cmd = new SqlCommand("IMPS_WebService_ReverseFeed_Updation", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@Respcode", statusCode);
                cmd.Parameters.AddWithValue("@Reason", StatusMessage);
                cmd.Parameters.AddWithValue("@Ihno", Ihno);
                cmd.Parameters.AddWithValue("@RRN", BankReferenceNumber);
                con.Open();
                cmd.ExecuteNonQuery();
            }

        }
        public void writelog(string ErrDesc, string ErrSrc, DateTime DateTime, string ErrIn, string callid)
        {
            StreamWriter errfile;
            StringBuilder sbError = new StringBuilder();
            if (!System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\LogFile"))
            {
                System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\LogFile");
            }
            errfile = File.AppendText(System.AppDomain.CurrentDomain.BaseDirectory + "\\LogFile\\Error" + DateTime.Now.ToString("ddMMMyyyy") + ".log");
            sbError.Append("Err Date:" + DateTime.Now.ToString());
            sbError.Append("User Info:" + "\t" + ErrDesc + "\r\n");
            sbError.Append("Err Desc:" + "\t" + ErrSrc + "\r\n");
            sbError.Append("Other Info:" + "\t" + DateTime + "\r\n");
            sbError.Append("Err In:" + "\t" + ErrIn + "\r\n");
            sbError.Append("Parameter Info: CallID " + "\r\n");
            sbError.Append("*********************************************************************" + "\r\n" + "\r\n");
            errfile.WriteLine(sbError.ToString());
            errfile.Close();
        }

    }
}
