using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using MySql.Data.MySqlClient;
using System.Net.Http;
using Newtonsoft.Json;

using System.Data;


using Amazon.Lambda.Core;

using System.IO;
using System.Security.Cryptography;
using System.Text;
using RestSharp;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DocuSign_Connector;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public string FunctionHandler(string input, ILambdaContext context)
    {

        
        string myConnectionString = "server=db6.cqc3tpt63rhe.us-west-1.rds.amazonaws.com;uid=StellarAdmin;pwd=Stellar1c;database=StellarOne";
        MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection();
        conn.ConnectionString = myConnectionString;

        MySql.Data.MySqlClient.MySqlConnection conn2 = new MySqlConnection();
        conn2.ConnectionString = myConnectionString;

        Console.WriteLine("Opening Connection");
        conn.Open();
        conn2.Open();
        string sqlHeader = "SELECT * FROM StellarOne.JiraBilling Where TaskStatus = 'UA Testing' and PhaseSignOffStatus = 'ToSend'";

        var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = sqlHeader;
        var sqlResult = cmd.ExecuteReaderAsync();
        sqlResult.Wait(0);



        while (sqlResult.Result.HasRows)
        {

            DocuSign.PostRequest.Rootobject envelope = new DocuSign.PostRequest.Rootobject();
            DocuSign.PostRequest.Document document = new DocuSign.PostRequest.Document();
            DocuSign.PostRequest.Recipients recipient = new DocuSign.PostRequest.Recipients();
            DocuSign.PostRequest.Signer singer = new DocuSign.PostRequest.Signer();
            DocuSign.PostRequest.Datesignedtab datesignedtab = new DocuSign.PostRequest.Datesignedtab();
            DocuSign.PostRequest.Initialheretab initialheretab = new DocuSign.PostRequest.Initialheretab();
            DocuSign.PostRequest.Signheretab signteretab = new DocuSign.PostRequest.Signheretab();
            DocuSign.PostRequest.Fullnametab fullnametab = new DocuSign.PostRequest.Fullnametab();
            DocuSign.PostRequest.Texttab texttab = new DocuSign.PostRequest.Texttab();
            DocuSign.PostRequest.Titletab titletab = new DocuSign.PostRequest.Titletab();
            DocuSign.PostRequest.Radiogrouptab radiogrouptab = new DocuSign.PostRequest.Radiogrouptab();
            DocuSign.PostRequest.Radio radio = new DocuSign.PostRequest.Radio();
            DocuSign.PostRequest.Tabs tabs = new DocuSign.PostRequest.Tabs();
            List<DocuSign.PostRequest.Radiogrouptab> grouptabs = new List<DocuSign.PostRequest.Radiogrouptab>();
            DocuSign.PostRequest.Eventnotification eventnotification = new DocuSign.PostRequest.Eventnotification();
            DocuSign.PostRequest.Envelopeevent envelopeevent = new DocuSign.PostRequest.Envelopeevent();    


            envelope.documents = new List<DocuSign.PostRequest.Document>();






            recipient.carbonCopies = new List<DocuSign.PostRequest.CarbonCopy>();


            envelope.recipients = recipient;
            string customer = "";
            string project = "";
             string completionDate = ""; //This is actually Testing Date

            sqlResult.Result.Read();
            eventnotification.envelopeEvents = new List<DocuSign.PostRequest.Envelopeevent>();
            envelopeevent.envelopeEventStatusCode = "completed";
            eventnotification.envelopeEvents.Add(envelopeevent);
            eventnotification.url = $"https://i77nj699mj.execute-api.us-west-1.amazonaws.com/default/DocuSign_Listener?RecID={sqlResult.Result.GetValue(sqlResult.Result.GetOrdinal("RecID")).ToString()}";
            //eventnotification.url = $"https://webhook.site/806ee066-52db-4fe8-b13b-8bbf676d5208?RecID={sqlResult.Result.GetValue(sqlResult.Result.GetOrdinal("RecID")).ToString()}";
        
            eventnotification.includeDocuments = "false";
            eventnotification.includeDocumentFields = "false";
            eventnotification.requireAcknowledgment = "true";

            envelope.eventNotification = eventnotification;

            String phaseID = "";
            {
                string sqlPhase = $"Select * from Jira_Phase Where PhaseName = '{sqlResult.Result.GetValue(sqlResult.Result.GetOrdinal("SignOffForm")).ToString()}'";
                customer = sqlResult.Result.GetValue(sqlResult.Result.GetOrdinal("Customer")).ToString();
                project = sqlResult.Result.GetValue(sqlResult.Result.GetOrdinal("Project")).ToString();
                completionDate = DateTime.Parse(sqlResult.Result.GetValue(sqlResult.Result.GetOrdinal("UATestingDate")).ToString()).ToString("dd/MM/yyyy");


                var cmdPhase = conn2.CreateCommand();
                cmdPhase.CommandType = CommandType.Text;
                cmdPhase.CommandText = sqlPhase;
                var sqlPhaseResult = cmdPhase.ExecuteReaderAsync();
                sqlPhaseResult.Wait(0);
                sqlPhaseResult.Result.Read();
                string emailSubject = sqlPhaseResult.Result.GetValue(sqlPhaseResult.Result.GetOrdinal("EmailSubject")).ToString().Replace("***CUSTOMER***", customer).Replace("***PROJECT***", project);
                string docName = sqlPhaseResult.Result.GetValue(sqlPhaseResult.Result.GetOrdinal("DocumentName")).ToString().Replace("***CUSTOMER***", customer).Replace("***PROJECT***", project);
                envelope.emailsubject = emailSubject;
                envelope.status = "sent";
                document.documentId = "1";
                document.name = docName;
                var str = (byte[])sqlPhaseResult.Result.GetValue(sqlPhaseResult.Result.GetOrdinal("Document"));
                document.documentBase64 = System.Text.Encoding.UTF8.GetString(str);
                //document.documentBase64 = System.Convert.ToBase64String(str);
                phaseID = sqlPhaseResult.Result.GetValue(sqlPhaseResult.Result.GetOrdinal("RecID")).ToString();
                sqlPhaseResult.Result.Close();
                envelope.documents.Add(document);
            }

            //Carbon Copies
            {
                var cmdCC = conn2.CreateCommand();
                cmdCC.CommandType = CommandType.Text;
                cmdCC.CommandText = "Select * from Jira_CarbonCopies";//Use the same carbon copies for all phases. Can update to have different CC for each phase. 
                var cmdCCResult = cmdCC.ExecuteReaderAsync();
                cmdCCResult.Wait(0);
                //cmdCCResult.Result.Read();  
                while (cmdCCResult.Result.Read())
                {
                    DocuSign.PostRequest.CarbonCopy carbonCopy = new DocuSign.PostRequest.CarbonCopy();
                    carbonCopy.email = cmdCCResult.Result.GetValue(cmdCCResult.Result.GetOrdinal("Email")).ToString();
                    carbonCopy.name = cmdCCResult.Result.GetValue(cmdCCResult.Result.GetOrdinal("Name")).ToString();
                    carbonCopy.recipientId = Convert.ToInt32(cmdCCResult.Result.GetValue(cmdCCResult.Result.GetOrdinal("RecipientID")));
                    carbonCopy.routingOrder = Convert.ToInt32(cmdCCResult.Result.GetValue(cmdCCResult.Result.GetOrdinal("RoutingNumber")));
                    recipient.carbonCopies.Add(carbonCopy);

                }

                cmdCCResult.Result.Close();
            }
            //Signers - Just a single signer for now. Signer defined on task
            DocuSign.PostRequest.Signer signer = new DocuSign.PostRequest.Signer();
            {


                signer.email = sqlResult.Result.GetValue(sqlResult.Result.GetOrdinal("CustomerEmail")).ToString();
                signer.name = sqlResult.Result.GetValue(sqlResult.Result.GetOrdinal("CustomerSigner")).ToString();
                signer.recipientId = 1;
                signer.routingOrder = 1;

                radiogrouptab.groupName = "GoNoGo";
                grouptabs.Add(radiogrouptab);

                tabs.radioGroupTabs = grouptabs;
                tabs.dateSignedTabs = new List<DocuSign.PostRequest.Datesignedtab>();
                tabs.fullNameTabs = new List<DocuSign.PostRequest.Fullnametab>();
                tabs.titleTabs = new List<DocuSign.PostRequest.Titletab>();
                tabs.textTabs = new List<DocuSign.PostRequest.Texttab>();
                tabs.signHereTabs = new List<DocuSign.PostRequest.Signheretab>();
                tabs.radioGroupTabs[0].radios = new List<DocuSign.PostRequest.Radio>();
                recipient.signers = new List<DocuSign.PostRequest.Signer>();





            }

            //Tabs
            {
                var cmdTabs = conn2.CreateCommand();
                cmdTabs.CommandType = CommandType.Text;
                cmdTabs.CommandText = @$"SELECT  Coalesce(xOffset,0) xOffset, Coalesce(yOffset,0) yOffset, coalesce(HorizontalAlignment,'') HorizontalAlignment, coalesce(AnchorString,'') AnchorString,
                                        coalesce(AnchorUnits,'') AnchorUnits, coalesce(Width,0) Width, coalesce(Font,'') Font, coalesce(FontSize,0) FontSize, TabType,RadioGroup,Coalesce(ReplacementValue,'') ReplacementValue
                                        from StellarOne.Jira_PhaseTabs Where PhaseID = {phaseID}";
                var cmdTabsResult = cmdTabs.ExecuteReaderAsync();
                cmdTabsResult.Wait(0);
                //cmdCCResult.Result.Read();  
                while (cmdTabsResult.Result.Read())
                {


                    switch (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("TabType")).ToString())
                    {

                        case "dateSignedTabs":
                            {
                                DocuSign.PostRequest.Datesignedtab tab = new DocuSign.PostRequest.Datesignedtab();
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("xOffset")).ToString() != "0")
                                {
                                    tab.anchorXOffset = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("xOffset")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("yOffset")).ToString() != "0")
                                {
                                    tab.anchorYOffset = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("yOffset")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("HorizontalAlignment")).ToString() != "")
                                {
                                    tab.anchorHorizontalAlignment = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("HorizontalAlignment")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorString")).ToString() != "")
                                {
                                    tab.anchorString = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorString")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorUnits")).ToString() != "")
                                {
                                    tab.anchorUnits = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorUnits")).ToString();
                                }
                                tabs.dateSignedTabs.Add(tab);
                            }
                            break;

                        case "fullNameTabs":


                            {
                                DocuSign.PostRequest.Fullnametab tab = new DocuSign.PostRequest.Fullnametab();
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("xOffset")).ToString() != "0")
                                {
                                    tab.anchorXOffset = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("xOffset")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("yOffset")).ToString() != "0")
                                {
                                    tab.anchorYOffset = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("yOffset")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("HorizontalAlignment")).ToString() != "")
                                {
                                    tab.anchorHorizontalAlignment = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("HorizontalAlignment")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorString")).ToString() != "")
                                {
                                    tab.anchorString = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorString")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorUnits")).ToString() != "")
                                {
                                    tab.anchorUnits = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorUnits")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("Width")).ToString() != "0")
                                {
                                    tab.width = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("Width")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("Font")).ToString() != "")
                                {
                                    tab.font = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("Font")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("FontSize")).ToString() != "0")
                                {
                                    tab.fontSize = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("FontSize")).ToString();
                                }
                                tabs.fullNameTabs.Add(tab);
                            }
                            break;

                        case "titleTabs":

                            {
                                DocuSign.PostRequest.Titletab tab = new DocuSign.PostRequest.Titletab();
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("xOffset")).ToString() != "0")
                                {
                                    tab.anchorXOffset = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("xOffset")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("yOffset")).ToString() != "0")
                                {
                                    tab.anchorYOffset = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("yOffset")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("HorizontalAlignment")).ToString() != "")
                                {
                                    tab.anchorHorizontalAlignment = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("HorizontalAlignment")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorString")).ToString() != "")
                                {
                                    tab.anchorString = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorString")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorUnits")).ToString() != "")
                                {
                                    tab.anchorUnits = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorUnits")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("Width")).ToString() != "0")
                                {
                                    tab.width = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("Width")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("Font")).ToString() != "")
                                {
                                    tab.font = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("Font")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("FontSize")).ToString() != "0")
                                {
                                    tab.fontSize = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("FontSize")).ToString();
                                }
                                tabs.titleTabs.Add(tab);

                            }
                            break;
                        case "textTabs":

                            {
                                DocuSign.PostRequest.Texttab tab = new DocuSign.PostRequest.Texttab();
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("xOffset")).ToString() != "0")
                                {
                                    tab.anchorXOffset = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("xOffset")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("yOffset")).ToString() != "0")
                                {
                                    tab.anchorYOffset = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("yOffset")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("HorizontalAlignment")).ToString() != "")
                                {
                                    tab.anchorHorizontalAlignment = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("HorizontalAlignment")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorString")).ToString() != "")
                                {
                                    tab.anchorString = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorString")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorUnits")).ToString() != "")
                                {
                                    tab.anchorUnits = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorUnits")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("Width")).ToString() != "0")
                                {
                                    tab.width = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("Width")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("Font")).ToString() != "")
                                {
                                    tab.font = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("Font")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("FontSize")).ToString() != "0")
                                {
                                    tab.fontSize = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("FontSize")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("ReplacementValue")).ToString() != "")
                                {
                                    tab.value = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("ReplacementValue")).ToString().Replace("***PROJECT***", project).Replace("***COMPLETIONDATE***", completionDate);
                                }
                                tabs.textTabs.Add(tab);

                            }
                            break;
                        case "signHereTabs":
                            {
                                DocuSign.PostRequest.Signheretab tab = new DocuSign.PostRequest.Signheretab();
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("xOffset")).ToString() != "0")
                                {
                                    tab.anchorXOffset = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("xOffset")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("yOffset")).ToString() != "0")
                                {
                                    tab.anchorYOffset = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("yOffset")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("HorizontalAlignment")).ToString() != "")
                                {
                                    tab.anchorHorizontalAlignment = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("HorizontalAlignment")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorString")).ToString() != "")
                                {
                                    tab.anchorString = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorString")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorUnits")).ToString() != "")
                                {
                                    tab.anchorUnits = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorUnits")).ToString();
                                }
                                tabs.signHereTabs.Add(tab);
                            }

                            break;
                        case "radioGroupTabs":
                            {
                                DocuSign.PostRequest.Radio tab = new DocuSign.PostRequest.Radio();
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("xOffset")).ToString() != "0")
                                {
                                    tab.anchorXOffset = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("xOffset")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("yOffset")).ToString() != "0")
                                {
                                    tab.anchorYOffset = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("yOffset")).ToString();
                                }

                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorString")).ToString() != "")
                                {
                                    tab.anchorString = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorString")).ToString();
                                }
                                if (cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorUnits")).ToString() != "")
                                {
                                    tab.anchorUnits = cmdTabsResult.Result.GetValue(cmdTabsResult.Result.GetOrdinal("AnchorUnits")).ToString();
                                }
                                tabs.radioGroupTabs[0].radios.Add(tab);
                            }
                            break;
                        default:
                            break;
                    }


                }
                cmdTabsResult.Result.Close();


            }

            signer.tabs = tabs;
            recipient.signers.Add(signer);
            envelope.recipients = recipient;
            string jsonPayload = JsonConvert.SerializeObject(envelope, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            string username = "";
            string pw = "";
            string key = "";

            {
                var cmdUserInfo = conn2.CreateCommand();
                cmdUserInfo.CommandType = CommandType.Text;
                cmdUserInfo.CommandText = @$"SELECT ConfigurationInfo FROM StellarOne.Environments Where EnvKey = 'DocuSignProd'";
                DocuSign.Authentication.Rootobject auth =  JsonConvert.DeserializeObject<DocuSign.Authentication.Rootobject>(cmdUserInfo.ExecuteScalar().ToString());
                username = auth.Username;
                pw = auth.Password;
                key = auth.IntegratorKey;
                
            }

            DocuSign ds = new DocuSign();
            string envelopeID = "";
            envelopeID =ds.PostDocument(jsonPayload, username, pw,key);
            if ((envelopeID ?? "") != "error")
            {
                var cmdUpdateStaging = conn2.CreateCommand();
                cmdUpdateStaging.CommandType = CommandType.Text;
                cmdUpdateStaging.CommandText = $"Update JiraBilling set PhaseSignOffStatus = 'Sent' Where RecID = {sqlResult.Result.GetValue(sqlResult.Result.GetOrdinal("RecID")).ToString()}";
                cmdUpdateStaging.ExecuteScalar();

            }
            else
            {

            }

            sqlResult.Result.NextResult();
        }

        return input.ToUpper();
    }
}

public class DocuSign
{

    public string PostDocument(string body, string  username, string password, string key)
    {
        try
        {
            var client = new RestClient("https://demo.docusign.net/restapi/v2.1/accounts/7773124/envelopes");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-DocuSign-Authentication", $"{{\"Username\":\"{username}\",\"Password\":\"{password}\",\"IntegratorKey\":\"{key}\"}}");
            request.AddHeader("Content-Type", "application/json");
            //var body = @"";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            DocuSign.PostResponse.Rootobject responseJSON = new PostResponse.Rootobject();
            responseJSON = JsonConvert.DeserializeObject<DocuSign.PostResponse.Rootobject>(response.Content);
            return responseJSON.envelopeId;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return "error";

        }




    }
    public class Authentication
    {

        public class Rootobject
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string IntegratorKey { get; set; }
        }

    }
    public class PostResponse
    {

        public class Rootobject
        {
            public string envelopeId { get; set; }
            public string uri { get; set; }
            public DateTime statusDateTime { get; set; }
            public string status { get; set; }
        }

    }
    public class PostRequest
    {

        public class Rootobject
        {
            public string emailsubject { get; set; }
            public string status { get; set; }
            public string emailBlurb { get; set; }
            public Eventnotification eventNotification { get; set; }
            public List<Document> documents { get; set; }
            public Recipients recipients { get; set; }
        }

        public class Eventnotification
        {
            public string url { get; set; }
            public string includeCertificateOfCompletion { get; set; }
            public string includeDocuments { get; set; }
            public string includeDocumentFields { get; set; }
            public string requireAcknowledgment { get; set; }
            public List<Envelopeevent> envelopeEvents { get; set; }
        }

        public class Envelopeevent
        {
            public string envelopeEventStatusCode { get; set; }
        }

        public class Recipients
        {
            public List<Signer> signers { get; set; }
            public List<CarbonCopy> carbonCopies { get; set; }
        }

        public class Signer
        {
            public string email { get; set; }
            public string name { get; set; }
            public int recipientId { get; set; }
            public int routingOrder { get; set; }
            public Tabs tabs { get; set; }
        }

        public class CarbonCopy
        {
            public string email { get; set; }
            public string name { get; set; }
            public int recipientId { get; set; }
            public int routingOrder { get; set; }
        }

        public class Tabs
        {
            public List<Datesignedtab> dateSignedTabs { get; set; }
            public List<Titletab> titleTabs { get; set; }
            public List<Texttab> textTabs { get; set; }
            public List<Radiogrouptab> radioGroupTabs { get; set; }
            public List<Initialheretab> initialHereTabs { get; set; }
            public List<Signheretab> signHereTabs { get; set; }
            public List<Fullnametab> fullNameTabs { get; set; }
        }

        public class Datesignedtab
        {
            public string anchorXOffset { get; set; }
            public string anchorHorizontalAlignment { get; set; }
            public string anchorString { get; set; }
            public string anchorYOffset { get; set; }
            public string anchorUnits { get; set; }
        }

        public class Titletab
        {
            public string anchorXOffset { get; set; }
            public string anchorHorizontalAlignment { get; set; }
            public string anchorString { get; set; }
            public string anchorYOffset { get; set; }
            public string anchorUnits { get; set; }
            public string width { get; set; }
            public string font { get; set; }
            public string fontSize { get; set; }
        }

        public class Texttab
        {
            public string anchorXOffset { get; set; }
            public string anchorHorizontalAlignment { get; set; }
            public string anchorString { get; set; }
            public string anchorYOffset { get; set; }
            public string anchorUnits { get; set; }
            public string width { get; set; }
            public string font { get; set; }
            public string fontSize { get; set; }
            public string anchorCaseSensitive { get; set; }
            public string value { get; set; }
        }

        public class Radiogrouptab
        {
            public string groupName { get; set; }
            public List<Radio> radios { get; set; }
        }

        public class Initialheretab
        {
            public string anchorXOffset { get; set; }
            public string anchorHorizontalAlignment { get; set; }
            public string anchorString { get; set; }
            public string anchorYOffset { get; set; }
            public string anchorUnits { get; set; }
        }

        public class Signheretab
        {
            public string anchorXOffset { get; set; }
            public string anchorHorizontalAlignment { get; set; }
            public string anchorString { get; set; }
            public string anchorYOffset { get; set; }
            public string anchorUnits { get; set; }
        }

        public class Fullnametab
        {
            public string anchorXOffset { get; set; }
            public string anchorHorizontalAlignment { get; set; }
            public string anchorString { get; set; }
            public string anchorYOffset { get; set; }
            public string anchorUnits { get; set; }
            public string width { get; set; }
            public string font { get; set; }
            public string fontSize { get; set; }
        }

        public class Radio
        {
            public string anchorXOffset { get; set; }
            public string anchorHorizontalAlignment { get; set; }
            public string anchorString { get; set; }
            public string anchorYOffset { get; set; }
            public string anchorUnits { get; set; }

        }

        public class Document
        {
            public string documentId { get; set; }
            public string name { get; set; }
            public string documentBase64 { get; set; }
        }

    }
}
