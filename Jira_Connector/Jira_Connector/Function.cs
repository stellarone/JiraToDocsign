using Amazon.Lambda.Core;
using System.Net.Http;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using System.Data;
using RestSharp;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Jira_Connector;

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

        var httpClientHandler = new HttpClientHandler();
        httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

        string myConnectionString = "server=db6.cqc3tpt63rhe.us-west-1.rds.amazonaws.com;uid=StellarAdmin;pwd=Stellar1c;database=StellarOne";
        MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();
        conn.ConnectionString = myConnectionString;

        conn.Open();

        //configInfoCmd.CommandType = CommandType.Text;
        //configInfoCmd.CommandText = $"Select ConfigurationInfo from Environments Where EnvKey='{envID}'";

        int startAt = 0;
        int issueCount = 100;

        while (issueCount == 100)
        {
            RestClient client = new RestClient($"https://stellarone.atlassian.net/rest/api/3/search?jql=\"Docusign\"=Yes&startAt={startAt}&maxResults=1000");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Basic cmljaGFyZEBzdGVsbGFyb25lY29uc3VsdGluZy5jb206VXJLS0JnMjkxMHc0ZEx2d1JUYW1EMDUx");
            request.AddHeader("Accept", "application/json");

            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            JIRA.GetResponse.Rootobject results = new JIRA.GetResponse.Rootobject();
            results = JsonConvert.DeserializeObject<JIRA.GetResponse.Rootobject>(response.Content.ToString());
            issueCount = results.issues.Count();
            startAt = startAt + 100;

            foreach (var issue in results.issues)
            {

                var cmdSQL = conn.CreateCommand();
                cmdSQL.CommandTimeout = 600;
                cmdSQL.CommandType = CommandType.StoredProcedure;
                cmdSQL.CommandText = "JiraBillingDetail";
                cmdSQL.Parameters.AddWithValue("_Customer", issue.fields.project.name.ToString());
                cmdSQL.Parameters.AddWithValue("_CardCode", issue.fields.project.key.ToString());
                if (issue.fields.fixVersions.Count() > 0)
                {
                    cmdSQL.Parameters.AddWithValue("_Project", issue.fields.fixVersions[0].name.ToString());
                }
                else
                {
                    cmdSQL.Parameters.AddWithValue("_Project", "");
                }
                if(issue.key == "C00000189-7")
                {
                    Console.WriteLine("Stop");
                }
                cmdSQL.Parameters.AddWithValue("_Task", issue.fields.customfield_10029?.value.ToString() ?? "");
                cmdSQL.Parameters.AddWithValue("_BacklogStatus", issue.fields.customfield_10030?.value?.ToString() ?? "");
                cmdSQL.Parameters.AddWithValue("_Status", issue.fields.status.name.ToString());
                cmdSQL.Parameters.AddWithValue("_DueDate", issue.fields.duedate?.ToString() ?? "");
                cmdSQL.Parameters.AddWithValue("_BillingType", issue.fields.customfield_10036?.value.ToString() ?? "");
                cmdSQL.Parameters.AddWithValue("_BillingNotes", issue.fields.customfield_10034?.ToString() ?? "");
                cmdSQL.Parameters.AddWithValue("_FixedFeeAmount", issue.fields.customfield_10035?.ToString() ?? "");
                cmdSQL.Parameters.AddWithValue("_BillingRate", issue.fields.customfield_10037?.ToString() ?? "");
                cmdSQL.Parameters.AddWithValue("_HoursEstimate", issue.fields.customfield_10038?.ToString() ?? "");
                cmdSQL.Parameters.AddWithValue("_Assignee", issue.fields.assignee?.displayName?.ToString() ?? "");
                cmdSQL.Parameters.AddWithValue("_CustomerSigner", issue.fields.customfield_10041?.ToString() ?? "");
                cmdSQL.Parameters.AddWithValue("_CustomerEmail", issue.fields.customfield_10042?.ToString() ?? "");
                cmdSQL.Parameters.AddWithValue("_Summary", issue.fields.summary?.ToString() ?? "");
                cmdSQL.Parameters.AddWithValue("_SignOffForm", issue.fields.customfield_10050?.value.ToString() ?? "");
                cmdSQL.Parameters.AddWithValue("_Milestone", issue.key);
                cmdSQL.Parameters.AddWithValue("_PM", issue.fields.customfield_10051?.displayName.ToString() ?? "Tommy Loyd");
                Console.WriteLine("Starting Insert");
                cmdSQL.ExecuteNonQuery();
            }

        }
        return input.ToUpper();
    }
}

public class JIRA
{
    public class GetResponse
    {

        public class Rootobject
        {
            public string expand { get; set; }
            public int startAt { get; set; }
            public int maxResults { get; set; }
            public int total { get; set; }
            public Issue[] issues { get; set; }
        }

        public class Issue
        {
            public string expand { get; set; }
            public string id { get; set; }
            public string self { get; set; }
            public string key { get; set; }
            public Fields fields { get; set; }
        }

        public class Fields
        {
            public DateTime statuscategorychangedate { get; set; }
            public Issuetype issuetype { get; set; }
            public object timespent { get; set; }
            public Customfield_10030 customfield_10030 { get; set; }
            public Project project { get; set; }
            public Customfield_10032 customfield_10032 { get; set; }
            public Fixversion[] fixVersions { get; set; }
            public Customfield_10033[] customfield_10033 { get; set; }
            public string customfield_10034 { get; set; }
            public object aggregatetimespent { get; set; }
            public float? customfield_10035 { get; set; }
            public Resolution resolution { get; set; }
            public Customfield_10036 customfield_10036 { get; set; }
            public float? customfield_10037 { get; set; }
            public float? customfield_10028 { get; set; }
            public Customfield_10029 customfield_10029 { get; set; }
            public DateTime? resolutiondate { get; set; }
            public int workratio { get; set; }
            public Watches watches { get; set; }
            public DateTime? lastViewed { get; set; }
            public DateTime created { get; set; }
            public object customfield_10020 { get; set; }
            public Customfield_10021[] customfield_10021 { get; set; }
            public DateTime? customfield_10022 { get; set; }
            public string customfield_10023 { get; set; }
            public Priority priority { get; set; }
            public object[] labels { get; set; }
            public object customfield_10016 { get; set; }
            public string customfield_10017 { get; set; }
            public Customfield_10018 customfield_10018 { get; set; }
            public string customfield_10019 { get; set; }
            public object timeestimate { get; set; }
            public object aggregatetimeoriginalestimate { get; set; }
            public object[] versions { get; set; }
            public Issuelink[] issuelinks { get; set; }
            public Assignee assignee { get; set; }
            public DateTime updated { get; set; }
            public Status status { get; set; }
            public object[] components { get; set; }
            public Customfield_10050 customfield_10050 { get; set; }
            public Customfield_10051 customfield_10051 { get; set; }
            public object timeoriginalestimate { get; set; }
            public object[] customfield_10052 { get; set; }
            public string customfield_10053 { get; set; }
            public Description description { get; set; }
            public object customfield_10010 { get; set; }
            public object customfield_10054 { get; set; }
            public object customfield_10055 { get; set; }
            public object customfield_10056 { get; set; }
            public object customfield_10057 { get; set; }
            public string customfield_10014 { get; set; }
            public string customfield_10015 { get; set; }
            public object customfield_10005 { get; set; }
            public Customfield_10049 customfield_10049 { get; set; }
            public object customfield_10006 { get; set; }
            public object security { get; set; }
            public object customfield_10007 { get; set; }
            public object customfield_10008 { get; set; }
            public object aggregatetimeestimate { get; set; }
            public object customfield_10009 { get; set; }
            public string summary { get; set; }
            public Creator creator { get; set; }
            public Subtask[] subtasks { get; set; }
            public float? customfield_10040 { get; set; }
            public string customfield_10041 { get; set; }
            public string customfield_10042 { get; set; }
            public Customfield_10043 customfield_10043 { get; set; }
            public Reporter reporter { get; set; }
            public Aggregateprogress aggregateprogress { get; set; }
            public object customfield_10044 { get; set; }
            public object customfield_10045 { get; set; }
            public object customfield_10001 { get; set; }
            public object customfield_10002 { get; set; }
            public string customfield_10046 { get; set; }
            public object customfield_10003 { get; set; }
            public object customfield_10047 { get; set; }
            public object customfield_10004 { get; set; }
            public float? customfield_10038 { get; set; }
            public Customfield_10039 customfield_10039 { get; set; }
            public object environment { get; set; }
            public string duedate { get; set; }
            public Progress progress { get; set; }
            public Votes votes { get; set; }
            public string customfield_10011 { get; set; }
            public Customfield_10012 customfield_10012 { get; set; }
            public string customfield_10013 { get; set; }
            public Parent parent { get; set; }
        }

        public class Issuetype
        {
            public string self { get; set; }
            public string id { get; set; }
            public string description { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public bool subtask { get; set; }
            public int avatarId { get; set; }
            public int hierarchyLevel { get; set; }
        }

        public class Customfield_10030
        {
            public string self { get; set; }
            public string value { get; set; }
            public string id { get; set; }
        }

        public class Project
        {
            public string self { get; set; }
            public string id { get; set; }
            public string key { get; set; }
            public string name { get; set; }
            public string projectTypeKey { get; set; }
            public bool simplified { get; set; }
            public Avatarurls avatarUrls { get; set; }
            public Projectcategory projectCategory { get; set; }
        }

        public class Avatarurls
        {
            public string _48x48 { get; set; }
            public string _24x24 { get; set; }
            public string _16x16 { get; set; }
            public string _32x32 { get; set; }
        }

        public class Projectcategory
        {
            public string self { get; set; }
            public string id { get; set; }
            public string description { get; set; }
            public string name { get; set; }
        }

        public class Customfield_10032
        {
            public int version { get; set; }
            public string type { get; set; }
            public Content[] content { get; set; }
        }

        public class Content
        {
            public string type { get; set; }
            public Content1[] content { get; set; }
        }

        public class Content1
        {
            public string type { get; set; }
            public string text { get; set; }
            public Content2[] content { get; set; }
        }

        public class Content2
        {
            public string type { get; set; }
            public Content3[] content { get; set; }
        }

        public class Content3
        {
            public string type { get; set; }
            public string text { get; set; }
        }

        public class Resolution
        {
            public string self { get; set; }
            public string id { get; set; }
            public string description { get; set; }
            public string name { get; set; }
        }

        public class Customfield_10036
        {
            public string self { get; set; }
            public string value { get; set; }
            public string id { get; set; }
        }

        public class Customfield_10029
        {
            public string self { get; set; }
            public string value { get; set; }
            public string id { get; set; }
        }

        public class Watches
        {
            public string self { get; set; }
            public int watchCount { get; set; }
            public bool isWatching { get; set; }
        }

        public class Priority
        {
            public string self { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public string id { get; set; }
        }

        public class Customfield_10018
        {
            public bool hasEpicLinkFieldDependency { get; set; }
            public bool showField { get; set; }
            public Noneditablereason nonEditableReason { get; set; }
        }

        public class Noneditablereason
        {
            public string reason { get; set; }
            public string message { get; set; }
        }

        public class Assignee
        {
            public string self { get; set; }
            public string accountId { get; set; }
            public Avatarurls1 avatarUrls { get; set; }
            public string displayName { get; set; }
            public bool active { get; set; }
            public string timeZone { get; set; }
            public string accountType { get; set; }
        }

        public class Avatarurls1
        {
            public string _48x48 { get; set; }
            public string _24x24 { get; set; }
            public string _16x16 { get; set; }
            public string _32x32 { get; set; }
        }

        public class Status
        {
            public string self { get; set; }
            public string description { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public string id { get; set; }
            public Statuscategory statusCategory { get; set; }
        }

        public class Statuscategory
        {
            public string self { get; set; }
            public int id { get; set; }
            public string key { get; set; }
            public string colorName { get; set; }
            public string name { get; set; }
        }

        public class Customfield_10050
        {
            public string self { get; set; }
            public string value { get; set; }
            public string id { get; set; }
        }

        public class Customfield_10051
        {
            public string self { get; set; }
            public string accountId { get; set; }
            public Avatarurls2 avatarUrls { get; set; }
            public string displayName { get; set; }
            public bool active { get; set; }
            public string timeZone { get; set; }
            public string accountType { get; set; }
        }

        public class Avatarurls2
        {
            public string _48x48 { get; set; }
            public string _24x24 { get; set; }
            public string _16x16 { get; set; }
            public string _32x32 { get; set; }
        }

        public class Description
        {
            public int version { get; set; }
            public string type { get; set; }
            public Content4[] content { get; set; }
        }

        public class Content4
        {
            public string type { get; set; }
            public Content5[] content { get; set; }
            public Attrs attrs { get; set; }
        }

        public class Attrs
        {
            public bool isNumberColumnEnabled { get; set; }
            public string layout { get; set; }
            public string localId { get; set; }
        }

        public class Content5
        {
            public string type { get; set; }
            public string text { get; set; }
            public Mark[] marks { get; set; }
            public Content6[] content { get; set; }
            public Attrs1 attrs { get; set; }
        }

        public class Attrs1
        {
            public string id { get; set; }
            public string type { get; set; }
            public string collection { get; set; }
            public string occurrenceKey { get; set; }
            public string text { get; set; }
            public string accessLevel { get; set; }
        }

        public class Mark
        {
            public string type { get; set; }
            public Attrs2 attrs { get; set; }
        }

        public class Attrs2
        {
            public string color { get; set; }
        }

        public class Content6
        {
            public string type { get; set; }
            public Content7[] content { get; set; }
            public Attrs3 attrs { get; set; }
        }

        public class Attrs3
        {
        }

        public class Content7
        {
            public string type { get; set; }
            public string text { get; set; }
            public Content8[] content { get; set; }
        }

        public class Content8
        {
            public string type { get; set; }
            public Content9[] content { get; set; }
            public string text { get; set; }
        }

        public class Content9
        {
            public string type { get; set; }
            public string text { get; set; }
            public Content10[] content { get; set; }
            public Mark1[] marks { get; set; }
        }

        public class Content10
        {
            public string type { get; set; }
            public Content11[] content { get; set; }
        }

        public class Content11
        {
            public string type { get; set; }
            public string text { get; set; }
        }

        public class Mark1
        {
            public string type { get; set; }
            public Attrs4 attrs { get; set; }
        }

        public class Attrs4
        {
            public string color { get; set; }
        }

        public class Customfield_10049
        {
            public string self { get; set; }
            public string value { get; set; }
            public string id { get; set; }
        }

        public class Creator
        {
            public string self { get; set; }
            public string accountId { get; set; }
            public Avatarurls3 avatarUrls { get; set; }
            public string displayName { get; set; }
            public bool active { get; set; }
            public string timeZone { get; set; }
            public string accountType { get; set; }
            public string emailAddress { get; set; }
        }

        public class Avatarurls3
        {
            public string _48x48 { get; set; }
            public string _24x24 { get; set; }
            public string _16x16 { get; set; }
            public string _32x32 { get; set; }
        }

        public class Customfield_10043
        {
            public string self { get; set; }
            public string value { get; set; }
            public string id { get; set; }
        }

        public class Reporter
        {
            public string self { get; set; }
            public string accountId { get; set; }
            public Avatarurls4 avatarUrls { get; set; }
            public string displayName { get; set; }
            public bool active { get; set; }
            public string timeZone { get; set; }
            public string accountType { get; set; }
            public string emailAddress { get; set; }
        }

        public class Avatarurls4
        {
            public string _48x48 { get; set; }
            public string _24x24 { get; set; }
            public string _16x16 { get; set; }
            public string _32x32 { get; set; }
        }

        public class Aggregateprogress
        {
            public int progress { get; set; }
            public int total { get; set; }
        }

        public class Customfield_10039
        {
            public string self { get; set; }
            public string accountId { get; set; }
            public string emailAddress { get; set; }
            public Avatarurls5 avatarUrls { get; set; }
            public string displayName { get; set; }
            public bool active { get; set; }
            public string timeZone { get; set; }
            public string accountType { get; set; }
        }

        public class Avatarurls5
        {
            public string _48x48 { get; set; }
            public string _24x24 { get; set; }
            public string _16x16 { get; set; }
            public string _32x32 { get; set; }
        }

        public class Progress
        {
            public int progress { get; set; }
            public int total { get; set; }
        }

        public class Votes
        {
            public string self { get; set; }
            public int votes { get; set; }
            public bool hasVoted { get; set; }
        }

        public class Customfield_10012
        {
            public string self { get; set; }
            public string value { get; set; }
            public string id { get; set; }
        }

        public class Parent
        {
            public string id { get; set; }
            public string key { get; set; }
            public string self { get; set; }
            public Fields1 fields { get; set; }
        }

        public class Fields1
        {
            public string summary { get; set; }
            public Status1 status { get; set; }
            public Priority1 priority { get; set; }
            public Issuetype1 issuetype { get; set; }
        }

        public class Status1
        {
            public string self { get; set; }
            public string description { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public string id { get; set; }
            public Statuscategory1 statusCategory { get; set; }
        }

        public class Statuscategory1
        {
            public string self { get; set; }
            public int id { get; set; }
            public string key { get; set; }
            public string colorName { get; set; }
            public string name { get; set; }
        }

        public class Priority1
        {
            public string self { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public string id { get; set; }
        }

        public class Issuetype1
        {
            public string self { get; set; }
            public string id { get; set; }
            public string description { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public bool subtask { get; set; }
            public int hierarchyLevel { get; set; }
        }

        public class Fixversion
        {
            public string self { get; set; }
            public string id { get; set; }
            public string description { get; set; }
            public string name { get; set; }
            public bool archived { get; set; }
            public bool released { get; set; }
            public string releaseDate { get; set; }
        }

        public class Customfield_10033
        {
            public string self { get; set; }
            public string value { get; set; }
            public string id { get; set; }
        }

        public class Customfield_10021
        {
            public string self { get; set; }
            public string value { get; set; }
            public string id { get; set; }
        }

        public class Issuelink
        {
            public string id { get; set; }
            public string self { get; set; }
            public Type type { get; set; }
            public Outwardissue outwardIssue { get; set; }
            public Inwardissue inwardIssue { get; set; }
        }

        public class Type
        {
            public string id { get; set; }
            public string name { get; set; }
            public string inward { get; set; }
            public string outward { get; set; }
            public string self { get; set; }
        }

        public class Outwardissue
        {
            public string id { get; set; }
            public string key { get; set; }
            public string self { get; set; }
            public Fields2 fields { get; set; }
        }

        public class Fields2
        {
            public string summary { get; set; }
            public Status2 status { get; set; }
            public Priority2 priority { get; set; }
            public Issuetype2 issuetype { get; set; }
        }

        public class Status2
        {
            public string self { get; set; }
            public string description { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public string id { get; set; }
            public Statuscategory2 statusCategory { get; set; }
        }

        public class Statuscategory2
        {
            public string self { get; set; }
            public int id { get; set; }
            public string key { get; set; }
            public string colorName { get; set; }
            public string name { get; set; }
        }

        public class Priority2
        {
            public string self { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public string id { get; set; }
        }

        public class Issuetype2
        {
            public string self { get; set; }
            public string id { get; set; }
            public string description { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public bool subtask { get; set; }
            public int avatarId { get; set; }
            public int hierarchyLevel { get; set; }
        }

        public class Inwardissue
        {
            public string id { get; set; }
            public string key { get; set; }
            public string self { get; set; }
            public Fields3 fields { get; set; }
        }

        public class Fields3
        {
            public string summary { get; set; }
            public Status3 status { get; set; }
            public Priority3 priority { get; set; }
            public Issuetype3 issuetype { get; set; }
        }

        public class Status3
        {
            public string self { get; set; }
            public string description { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public string id { get; set; }
            public Statuscategory3 statusCategory { get; set; }
        }

        public class Statuscategory3
        {
            public string self { get; set; }
            public int id { get; set; }
            public string key { get; set; }
            public string colorName { get; set; }
            public string name { get; set; }
        }

        public class Priority3
        {
            public string self { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public string id { get; set; }
        }

        public class Issuetype3
        {
            public string self { get; set; }
            public string id { get; set; }
            public string description { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public bool subtask { get; set; }
            public int hierarchyLevel { get; set; }
            public int avatarId { get; set; }
        }

        public class Subtask
        {
            public string id { get; set; }
            public string key { get; set; }
            public string self { get; set; }
            public Fields4 fields { get; set; }
        }

        public class Fields4
        {
            public string summary { get; set; }
            public Status4 status { get; set; }
            public Priority4 priority { get; set; }
            public Issuetype4 issuetype { get; set; }
        }

        public class Status4
        {
            public string self { get; set; }
            public string description { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public string id { get; set; }
            public Statuscategory4 statusCategory { get; set; }
        }

        public class Statuscategory4
        {
            public string self { get; set; }
            public int id { get; set; }
            public string key { get; set; }
            public string colorName { get; set; }
            public string name { get; set; }
        }

        public class Priority4
        {
            public string self { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public string id { get; set; }
        }

        public class Issuetype4
        {
            public string self { get; set; }
            public string id { get; set; }
            public string description { get; set; }
            public string iconUrl { get; set; }
            public string name { get; set; }
            public bool subtask { get; set; }
            public int avatarId { get; set; }
            public int hierarchyLevel { get; set; }
        }

    }
}




public class Rootobject
{
    public Fulfillment_Orders[] fulfillment_orders { get; set; }
}

public class Fulfillment_Orders
{
    public long id { get; set; }
    public long shop_id { get; set; }
    public long order_id { get; set; }
    public long assigned_location_id { get; set; }
    public string request_status { get; set; }
    public string status { get; set; }
    public string[] supported_actions { get; set; }
    public Destination destination { get; set; }
    public Line_Items[] line_items { get; set; }
    public DateTime fulfill_at { get; set; }
    public object international_duties { get; set; }
    public object[] fulfillment_holds { get; set; }
    public Delivery_Method delivery_method { get; set; }
    public Assigned_Location assigned_location { get; set; }
    public object[] merchant_requests { get; set; }
}

public class Destination
{
    public long id { get; set; }
    public string address1 { get; set; }
    public string address2 { get; set; }
    public string city { get; set; }
    public object company { get; set; }
    public string country { get; set; }
    public string email { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string phone { get; set; }
    public string province { get; set; }
    public string zip { get; set; }
}

public class Delivery_Method
{
    public long id { get; set; }
    public string method_type { get; set; }
}

public class Assigned_Location
{
    public string address1 { get; set; }
    public string address2 { get; set; }
    public string city { get; set; }
    public string country_code { get; set; }
    public long location_id { get; set; }
    public string name { get; set; }
    public string phone { get; set; }
    public string province { get; set; }
    public string zip { get; set; }
}

public class Line_Items
{
    public long id { get; set; }
    public long shop_id { get; set; }
    public long fulfillment_order_id { get; set; }
    public int quantity { get; set; }
    public long line_item_id { get; set; }
    public long inventory_item_id { get; set; }
    public int fulfillable_quantity { get; set; }
    public long variant_id { get; set; }
}



