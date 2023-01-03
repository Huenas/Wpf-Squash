using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp_Squash.HttpClient;
using WpfApp_Squash.Model;
using ArrayToExcel;
using System.IO;
using Microsoft.Win32;
using DevExpress.Utils.CommonDialogs.Internal;
using Microsoft.Office.Interop.Excel;
using Button = System.Windows.Controls.Button;
using WpfApp_Squash.ViewModel;
using Syroot.Windows.IO;

namespace WpfApp_Squash.View
{
    /// <summary>
    /// Logique d'interaction pour CampagneCGI.xaml
    /// </summary>
    public partial class CampagneCgi : UserControl
    {
        List<Campagne> campagnes = new List<Campagne>();
        List<Evol> evolss = new List<Evol>();
        
        public CampagneCgi()
        {
            
            InitializeComponent();
            //Base url of the http calls
            string baseUrl = "https://saas-see01.henix.com/squash/api/rest/latest/";

            RestClient rClient = new RestClient(baseUrl);

            //Get all the campaigns folders (~ 530) since we don't have access right 
            rClient.endPoint = "https://saas-see01.henix.com/squash/api/rest/latest/campaign-folders";
            string jsonResponse = string.Empty;
            jsonResponse = rClient.makeRequest("/campaign-folders?page=0&size=530");
            int campagneFolderId;
            int aprId;
            dynamic dynJson = JsonConvert.DeserializeObject(jsonResponse);

            //loop through each campaign folders and get its id and name
            foreach (var item in dynJson["_embedded"]["campaign-folders"])
               
            {                     
                string checkItem = item.name;
                int idCampagne = item.id;
                //We created a list of the campaign we wanted to check in the Campaign class, where a campaign has an ID and a name
                //we then check if the campaign we have is a campaign we wanna check 
                List<Campagne> listCampagneToCheck = Campagne.listCampagne;
                bool isCampaign = listCampagneToCheck.Any(x => x.CampagneName == checkItem && x.IdCampagne == idCampagne);

                //Let's initialize the result of the inventory on 0 for EACH campaign
                int isSuccess = 0;
                int isFailed = 0;
                int isRunning = 0;
                
                //if the campaign we get from the previous loop, is a campaign contained in the list in the class campaign, it's set to TRUE due to the previous bool "iscampaign"
                if (isCampaign == true)
                {                    
                    //We get the campaign name and its ID
                    Campagne addcampagne = new Campagne();
                    addcampagne.Campagnes = item.name;
                    string campagnename = item.name;
                    addcampagne.Apr = item.name;
                    Console.WriteLine(addcampagne.Campagnes);
                                                       
                    //We created a list linked to the dynamic listview in the XAML menu. 
                    ListViewCampagne.Items.Add(addcampagne.Campagnes);                    
                    List<string> listEvol = new List<string>();
                    
                    //we then will browse the tree :
                    //Might be better (in terms of performance and understanding to make a recursive function depending of the ._type (in the json) of folder we get
                    //Since we found the tree quite difficult to understand, we made a foreach loop.
                    
                    campagneFolderId = item.id;
                    string inventaireUri = campagneFolderId + "/content";
                    string jsonResponse2 = string.Empty;

                    // To get the content of a campaign, we need the following call:  
                    // BaseUrl + campaign-folders + campaignId + /content 
                    jsonResponse2 = rClient.makeRequest("campaign-folders/" + inventaireUri);
                    dynamic dynJson2 = JsonConvert.DeserializeObject(jsonResponse2);
                    
                    //result:
                    foreach (var item2 in dynJson2["_embedded"]["content"])
                    {
                        //for each content of the campaign, we check weither it has subcampaign folders or if it has results directly in it (campaigns)
                        aprId = item2.id;
                        int resultAprId = aprId;
                        string containsCampaignFolder = item2._type;

                        //if it has SubFolders
                        #region if content has SubFolders
                        if (containsCampaignFolder.Contains("campaign-folder"))
                        {
                            string aprIdUri = aprId + "/content";
                            string jsonResponse5 = string.Empty;
                            jsonResponse5 = rClient.makeRequest("campaign-folders/" + aprIdUri);
                            dynamic dynJson5 = JsonConvert.DeserializeObject(jsonResponse5);
                            if (dynJson5["_embedded"] != null)
                            {
                                //loop through all the content in the campaigns
                                //ex: Campaigns -> Campaignsfolder/content -> campaigns content  
                                //Evols: 
                                foreach (var item5 in dynJson5["_embedded"]["content"])
                                {
                                    // We set the results of the tests set to 0, that will be set for EACH Evol
                                    int successEvol = 0;
                                    int failedEvol = 0;
                                    int runningEvol = 0;
                                    string campaignId = item5.id;
                                    string evolname = item5.name;
                                    listEvol.Add(evolname);
                                    List<int> listResultEvol = new List<int>();

                                    //if Evols contains SubFolders:
                                    #region Evols contains SubFolders
                                    string containsCampaignFolder2 = item5._type;
                                    if (containsCampaignFolder2.Contains("campaign-folder"))
                                    {
                                        //we loop through each subfolders of the Evol (same call as a campaign-folder) 
                                        string campaignIdUri = item5.id + "/content";
                                        string jsonResponse8 = string.Empty;
                                        jsonResponse8 = rClient.makeRequest("campaign-folders/" + campaignIdUri);
                                        dynamic dynJson8 = JsonConvert.DeserializeObject(jsonResponse8);
                                        if (dynJson8["_embedded"] != null)
                                        {
                                            foreach (var item8 in dynJson8["_embedded"]["content"])
                                            {
                                                //we then loop through each iteration of the subfolders, each iterations contains a test plan with tests results
                                                int iditem8 = item8.id;
                                                string jsonResponse6 = string.Empty;
                                                jsonResponse6 = rClient.makeRequest("campaigns/" + iditem8);
                                                dynamic dynJson6 = JsonConvert.DeserializeObject(jsonResponse6);
                                                foreach (var item6 in dynJson6["iterations"])
                                                {
                                                    string iterationId = item6.id;
                                                    string jsonResponse7 = string.Empty;
                                                    jsonResponse7 = rClient.makeRequest("iterations/" + iterationId + "/test-plan?page=0&size=100");
                                                    dynamic dynJson7 = JsonConvert.DeserializeObject(jsonResponse7);
                                                    //we set increment the variables of the results of the general campaign (isSuccess) 
                                                    // and increment the variables of the results of the current EVOL 
                                                    // /!\ we noticed some of the result might not be check for some campaigns 
                                                    if (dynJson7["_embedded"] != null)
                                                    {
                                                        var item7 = dynJson7["_embedded"]["test-plan"];
                                                        foreach (var item9 in dynJson7["_embedded"]["test-plan"])
                                                        {
                                                            int idIter = item9.id;
                                                            string resultUri = idIter + "/test-plan";
                                                            string executionStatus = item9.execution_status;
                                                            if (executionStatus == "SUCCESS")
                                                            {
                                                                isSuccess += 1;
                                                                successEvol += 1;
                                                            }
                                                            if (executionStatus == "FAILED")
                                                            {
                                                                isFailed += 1;
                                                                failedEvol += 1;
                                                            }
                                                            if (executionStatus == "RUNNING")
                                                            {
                                                                isRunning += 1;
                                                                runningEvol += 1;
                                                            }   

                                                            
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                        //For each EVOL, we then add its campaign in which it's attached, its results and its name
                                        this.evolss.Add(new Evol()
                                        {
                                            CampagneName = campagnename,
                                            Success = successEvol,
                                            Failed = failedEvol,
                                            Running = runningEvol,
                                            Evoleees = evolname
                                        });
                                    }
                                    #endregion Evol Contains Subfolders
                                    #region Evol contains campaign
                                    if (containsCampaignFolder2 == "campaign")
                                    {
                                        //we then loop through each iteration of the campaigns, each iterations contains a test plan with tests results
                                        string jsonResponse6 = string.Empty;
                                        jsonResponse6 = rClient.makeRequest("campaigns/" + campaignId);
                                        dynamic dynJson6 = JsonConvert.DeserializeObject(jsonResponse6);
                                        foreach (var item6 in dynJson6["iterations"])
                                        {   
                                            string iterationId = item6.id;
                                            string jsonResponse7 = string.Empty;
                                            jsonResponse7 = rClient.makeRequest("iterations/" + iterationId + "/test-plan?page=0&size=100");
                                            dynamic dynJson7 = JsonConvert.DeserializeObject(jsonResponse7);

                                            if (dynJson7["_embedded"] != null)
                                            {
                                                //we set increment the variables of the results of the general campaign (isSuccess) 
                                                // and increment the variables of the results of the current EVOL 
                                                // /!\ we noticed some of the result might not be check for some campaigns
                                                var item7 = dynJson7["_embedded"]["test-plan"];
                                                foreach (var item8 in dynJson7["_embedded"]["test-plan"])
                                                {
                                                    int idIter = item8.id;
                                                    string resultUri = idIter + "/test-plan";
                                                    string executionStatus = item8.execution_status;
                                                    if (executionStatus == "SUCCESS")
                                                    {
                                                        isSuccess += 1;
                                                        successEvol += 1;
                                                    }
                                                    if (executionStatus == "FAILED")
                                                    {
                                                        isFailed += 1;
                                                        failedEvol += 1;
                                                    }
                                                    if (executionStatus == "RUNNING")
                                                    {
                                                        isRunning += 1;
                                                        runningEvol += 1;
                                                    }                                                    
                                                }
                                            }

                                        }
                                        //For each EVOL, we then add its campaign in which it's attached, its results and its name
                                        this.evolss.Add(new Evol()
                                        {
                                            CampagneName = campagnename,
                                            Success = successEvol,
                                            Failed = failedEvol,
                                            Running = runningEvol,
                                            Evoleees = evolname
                                        });
                                    }
                                    #endregion Evol contains campaign
                                }
                            }
                        }
                        #endregion Has SubFolders
                        //If it contains only campaigns:
                        #region if content has only Campaigns
                        else
                        {
                            string jsonResponse3 = string.Empty;
                            jsonResponse3 = rClient.makeRequest("campaigns/" + resultAprId);
                            dynamic dynJson3 = JsonConvert.DeserializeObject(jsonResponse3);
                            string evolname = item2.name;
                            int successEvol=0;
                            int failedEvol=0;
                            int runningEvol=0;
                            foreach (var item3 in dynJson3["iterations"])
                            {
                                //we then loop through each iteration of the campaigns, each iterations contains a test plan with tests results
                                string campaignId = item3.id;
                                string iterationIdUri = campaignId + "/test-plan?page=0&size=100";
                                string jsonResponse4 = string.Empty;
                                jsonResponse4 = rClient.makeRequest("iterations/" + iterationIdUri);
                                dynamic dynJson4 = JsonConvert.DeserializeObject(jsonResponse4);
                                if (dynJson4["_embedded"] != null)
                                {
                                    //we set increment the variables of the results of the general campaign (isSuccess) 
                                    // and increment the variables of the results of the current EVOL 
                                    // /!\ we noticed some of the result might not be check for some campaigns 
                                    foreach (var item4 in dynJson4["_embedded"]["test-plan"])
                                    {
                                        int idIter = item4.id;
                                        string resultUri = idIter + "/test-plan";
                                        string executionStatus = item4.execution_status;
                                        if (executionStatus == "SUCCESS")
                                        {
                                            isSuccess += 1;
                                            successEvol += 1;
                                        }
                                        if (executionStatus == "FAILED")
                                        {
                                            isFailed += 1;
                                            failedEvol += 1;
                                        }
                                        if (executionStatus == "RUNNING")
                                        {
                                            isRunning += 1;
                                            runningEvol += 1;
                                        }                                      
                                    }
                                }
                            }
                            //For each EVOL, we then add its campaign in which it's attached, its results and its name
                            this.evolss.Add(new Evol()
                            {
                                CampagneName = campagnename,
                                Success = successEvol,
                                Failed = failedEvol,
                                Running = runningEvol,
                                Evoleees = evolname
                            });
                        }
                        #endregion has only campaigns
                    }
                    // For each CAMPAIGN, we then add its name, Overall Success, failed, running tests
                    this.campagnes.Add(new Campagne()
                    {
                        CampagneName = campagnename,
                        Success = isSuccess,
                        Failed = isFailed,
                        Running = isRunning,
                       
                    });                   
                    Console.WriteLine(isSuccess);
                    Console.WriteLine(isFailed);
                    Console.WriteLine(isRunning); 
                }               
            }
        }
        

        private void DownloadResult_Click(object sender, RoutedEventArgs e)
        {
            //when the user click on the button on the UI CampagneCgi.xaml, we get the name of the campaign by getting the button parameter
            Button b = sender as Button;           
            var campagnename = b.CommandParameter;           
            var campagne = campagnes.Where(item => item.CampagneName.Equals(campagnename)).FirstOrDefault();            

            //we then get the overall campaign results 
            int Succeded = campagne.Success;
            int failed = campagne.Failed;
            int running = campagne.Running;
            string campagneName = campagne.CampagneName;
  
            //create a new app Excel 
            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            app.Visible = true;
            app.WindowState = XlWindowState.xlMaximized;

            Workbook wb = app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            Worksheet ws = wb.Worksheets[1];
   
            //Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            //we then populate the excels like so (very intuitive):
            ws.Range["A1"].Value = "Inventaire des tests de la campagne " + campagneName + ":";                       
            ws.Range["A3"].Value = "Succès:";
            ws.Range["A4"].Value = Succeded;
            ws.Range["B3"].Value = "Echec:";
            ws.Range["B4"].Value = failed;
            ws.Range["C3"].Value = "En cours:";
            ws.Range["C4"].Value = running;

            //we initiade the cells where its going to be placed
            string cellNameEvol;
            string cellNameFailed;
            string cellNameSuccess;
            string cellNameRunning;
            //since the results of the main campaign we be displayed in A1-A4,B1-B4... we want to display EVOL belows, so in line 5
            int counter = 5;
            foreach (var evols in evolss.Where(item1 => item1.CampagneName.Equals(campagnename)))
            {
                //column A, B, C, D for result 
                cellNameFailed = "B" + counter.ToString();
                cellNameSuccess = "A" + counter.ToString();
                cellNameRunning = "C" + counter.ToString();
                cellNameEvol = "D" + counter.ToString();

                //set range for Evols
                var rangeEvol = ws.get_Range(cellNameEvol, cellNameEvol);
                var rangeSuccess = ws.get_Range(cellNameSuccess, cellNameSuccess);
                var rangeFailed = ws.get_Range(cellNameFailed, cellNameFailed);
                var rangeRunning = ws.get_Range(cellNameRunning, cellNameRunning);

                //Set results of values to the range
                rangeEvol.Value2 = "'" + evols.Evoleees.ToString() + "'";
                rangeSuccess.Value2 = evols.Success;
                rangeFailed.Value2 = evols.Failed;
                rangeRunning.Value2 = evols.Running;
                ++counter;

            }
            string downloadsPath = KnownFolders.Downloads.Path;
            //and to finish, we save the file in "C:\\users" (to be modified) with the campaign name, and its date (Year,month,day, hour,minute, second format)
            //
            wb.SaveAs(downloadsPath + campagneName + "_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".xlsx");
        }
    }

}
