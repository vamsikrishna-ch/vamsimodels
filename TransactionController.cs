using DataAccessLayer;
using System;
using System.Configuration;
using System.Data;
using System.Net;
using System.Web.Http;

namespace Axis_InstaRedemption.Controllers
{
    public class TransactionController : ApiController
    {
        Common c = new Common();
        DataSet ds = new DataSet();
        [HttpPost]
        public string DoTransaction(string ReqType, string Ihno, string Ifsc, string BeneAcno, string Amount, string RemittorName)
        {
            try
            {
                string url = "";
                ICSharpCode.SharpZipLib.Checksum.Adler32 achk = new ICSharpCode.SharpZipLib.Checksum.Adler32();

                if (ReqType == "R")
                {
                    achk.Update(System.Text.Encoding.UTF8.GetBytes(Ihno + "R" + "918291048390" + "9211000"
                                + "917020002179060" + RemittorName + Ifsc + BeneAcno + Amount + "remarks" + "INET" + "14022017"));

                    url = ConfigurationManager.AppSettings["ImpsAxisapiurl"].ToString() + "MOB_SERVICE_PROVIDER_ID=AXISMF&REQUEST_ID=" + Ihno
                 + "&REQUEST_TYPE=R" + "&REMITTOR_MOBILE_NUMBER=918291048390"
                 + "&REMITTOR_MMID=9211000&REMITTOR_ACCNT_NUM=917020002179060&BENE_IFSC=" + Ifsc + "&"
                 + "BENE_ACCNT_NUM=" + BeneAcno + "&AMOUNT=" + Amount + "&TRAN_INIT_CHANNEL_NAME=INET&CHECKSUM=" + achk.Value.ToString()
                 + "&REMITTOR_NAME=" + RemittorName + "&REMARKS=remarks";
                }

                if (ReqType == "E")
                {
                    achk.Update(System.Text.Encoding.UTF8.GetBytes(Ihno + "E" + "918291048390" + "9211000" + "14022017"));

                    url = ConfigurationManager.AppSettings["ImpsAxisapiurl"].ToString() + "?MOB_SERVICE_PROVIDER_ID=AXISMF&REQUEST_ID=" + Ihno
                 + "&REQUEST_TYPE=R" + "&REMITTOR_MOBILE_NUMBER=918291048390"
                 + "&REMITTOR_MMID=9211000&REMITTOR_ACCNT_NUM=917020002179060&BENE_IFSC=" + Ifsc + "&"
                 + "BENE_ACCNT_NUM=" + BeneAcno + "&AMOUNT=" + Amount + "&TRAN_INIT_CHANNEL_NAME=INET&CHECKSUM=" + achk.Value.ToString()
                 + "&REMITTOR_NAME=" + RemittorName + "&REMARKS=remarks";

                }

                ds = c.getInserlogrequest(url, Ihno);
                string data;
                using (WebClient webClient = new WebClient())
                {
                    data = webClient.DownloadString(url);
                    string[] lstResponse;
                    lstResponse = data.Split('|');
                    //Inserting ReverseFeed
                    c.InsertResponse(lstResponse[0].ToString(), lstResponse[1].ToString(), Ihno, lstResponse[3].ToString(), lstResponse[6].ToString());
                    //Inserting InternalLog
                    c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), data.ToString());
                    return data;
                }

            }
            catch (Exception ex)
            {
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), ex.Message.ToString());
                c.writelog(ex.Message, "", DateTime.Now, "", "");
                return ex.Message;
            }

        }
    }
}
