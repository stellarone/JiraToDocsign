using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using MySql.Data.MySqlClient;
using System.Net.Http;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using System.Data;


using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using RestSharp;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]


namespace Docusign_Listener;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public string FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        //Console.WriteLine(input.ToString());
        //Console.WriteLine(input.Body.ToString());
        //Console.WriteLine(input.Headers.ToString());

        //Get RecID from URL
        try
        {
            string recID = "";
            foreach (var param in input.QueryStringParameters)
            {
                if (param.Key == "RecID")
                {
                    recID = param.Value;
                }
                Console.WriteLine(recID);
            }

            string myConnectionString = "server=db6.cqc3tpt63rhe.us-west-1.rds.amazonaws.com;uid=StellarAdmin;pwd=Stellar1c;database=StellarOne";
            MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection();
            conn.ConnectionString = myConnectionString;
            conn.Open();

            //Update Record in MySQL
            {
                string sqlUpdateRecord = $"Update JiraBilling Set PhaseSignOffStatus = 'Complete' Where RecID = {recID}";
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sqlUpdateRecord;
                    var sqlResult = cmd.ExecuteScalar();
                    //sqlResult.Wait(0);
                }
            }

            //Get Jira ID
            string milestone = "";
            string sqlGetMilestone = $"Select Milestone from JiraBilling Where RecID = {recID}";
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlGetMilestone;
                milestone = cmd.ExecuteScalar().ToString();
                //sqlResult.Wait(0);

            }


            //Transition to Ready for Production
            //Disable for now 01/4/2024 so that We can manually flip for a while
            ////var client = new RestClient($"https://stellarone.atlassian.net/rest/api/2/issue/{milestone}/transitions");
            ////client.Timeout = -1;
            ////var request = new RestRequest(Method.POST);
            ////request.AddHeader("Authorization", "Basic cmljaGFyZEBzdGVsbGFyb25lY29uc3VsdGluZy5jb206VXJLS0JnMjkxMHc0ZEx2d1JUYW1EMDUx");
            ////request.AddHeader("Accept", "application/json");
            
            ////request.AddHeader("Content-Type", "application/json");
            
            ////var body = "{\"transition\": {\"id\": \"71\"}}";
            ////Console.WriteLine(body);
            ////request.AddParameter("application/json", body, ParameterType.RequestBody);
            ////Console.WriteLine("Executing Transition");
            ////var response = client.Execute(request);
            ////Console.WriteLine(response.Content);

        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            return "";
        }

        var resp = new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = "{}"
        };
        return "";
    }
}
