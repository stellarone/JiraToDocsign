using RestSharp;

namespace TestByDesignSessions
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var client = new RestClient("https://my359661.sapbydesign.com/sap/byd/odata/ana_businessanalytics_analytics.svc/RPZ422AAAB778FC4B57D8E6A2QueryResults?$select=FCY4BG2IZBY_141253ED8C,RCY4BG2IZBY_141253ED8C,CSP_CDT_F_DATE,CDOC_UUID,CY4BG2IZBY_141253ED8C_CURR,TY4BG2IZBY_141253ED8C_CURR,CDOC_INV_DATE,KCINV_AMOUNT_DUE_CONTENT,KCOVERDUE_DAYS,KCY4BG2IZBY_141253ED8C,KCITM_GR_AM_RC&$filter=(CDOC_STA_RELEASE eq '3') and (CDOC_CANC_IND eq false)  and (CDPY_BUYER_UUID eq '37618' )");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", "Basic RlNURUxMQVJPTkU6V2VsY29tZTE=");
            request.AddHeader("Cookie", "MYSAPSSO2=AjQxMDMBABhLAEEATwBSAFAANQBOAFMARgBJAEsAIAACAAYzADQAOAADABBHAFMAMgAgACAAIAAgACAABAAYMgAwADIAMgAwADcAMQAxADIAMAAyADUABQAEAAAACAYAAlgACQACRQD%2fAPswgfgGCSqGSIb3DQEHAqCB6jCB5wIBATELMAkGBSsOAwIaBQAwCwYJKoZIhvcNAQcBMYHHMIHEAgEBMBkwDjEMMAoGA1UEAxMDR1MyAgcgEQcnA1IVMAkGBSsOAwIaBQCgXTAYBgkqhkiG9w0BCQMxCwYJKoZIhvcNAQcBMBwGCSqGSIb3DQEJBTEPFw0yMjA3MTEyMDI1NTRaMCMGCSqGSIb3DQEJBDEWBBSMr47TIsdLSevSAFmI7CXSbgiylTAJBgcqhkjOOAQDBC8wLQIUMCShUfKl0f0MBiIfYO498V4cMBICFQCSrWH1eHPcgKZSMv7ttt%2f3Lh8VYQ%3d%3d; SAP_SESSIONID_GS2_348=Hr_GE9vcvJDMj6A2fXTwkn5sanEB-BHtrXv6Fj5AD-Q%3d; sap-usercontext=sap-client=348");
            IRestResponse response = client.Execute(request);
           MessageBox.Show(response.Content);


             response = client.Execute(request);
            MessageBox.Show(response.Content);
        }
    }
}