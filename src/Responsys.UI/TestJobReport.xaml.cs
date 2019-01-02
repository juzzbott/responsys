using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Responsys.UI.Services;

namespace Enivate.ResponseHub.Responsys.UI
{
    /// <summary>
    /// Interaction logic for TestJobReport.xaml
    /// </summary>
    public partial class TestJobReport : Window
    {

        private Unit _currentUnit;

        protected ResponseHubApiService MessageService;
        protected PrintedMessageRecordService PrintRecordService;

        public TestJobReport()
        {

            MessageService = new ResponseHubApiService();
            PrintRecordService = new PrintedMessageRecordService();

            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            JobMessage job = null;

            await LoadUnitDetails();

            IList<JobMessage> latestList = await MessageService.GetLatestMessages(_currentUnit.Id);

            // Loop through the jobs, skip any that start with RE:, as it's not actually a job
            foreach (JobMessage message in latestList)
            {
                if (message.MessageContent.StartsWith("RE:", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                job = message;
                break;
            }

            Dictionary<string, Style> styles = new Dictionary<string, Style>();
            styles.Add("ReportJobNumber", (Style)FindResource("ReportJobNumber"));
            styles.Add("ReportJobNumberEmergency", (Style)FindResource("ReportJobNumberEmergency"));
            styles.Add("ReportTimestamp", (Style)FindResource("ReportTimestamp"));
            styles.Add("ReportMapReferenceParagraph", (Style)FindResource("ReportMapReferenceParagraph"));
            styles.Add("ReportMapReference", (Style)FindResource("ReportMapReference"));
            styles.Add("ReportAddress", (Style)FindResource("ReportAddress"));
            styles.Add("ReportMapImage", (Style)FindResource("ReportMapImage"));
            styles.Add("ReportMapImageContainer", (Style)FindResource("ReportMapImageContainer"));
            styles.Add("ReportMessageContent", (Style)FindResource("ReportMessageContent"));
            FlowDocument flowDoc = PrintService.GetFormattedDocument(job, "/Responsys;component/assets/test-map-image.png", styles);

            flowScroller.Document = flowDoc;
        }

        private async Task LoadUnitDetails()
        {
            // Get the unit id from the application configuration
            Guid unitId = Guid.Empty;

            try
            {
                unitId = new Guid(ConfigurationManager.AppSettings["UnitId"]);
            }
            catch (Exception)
            {
                MessageBox.Show("The Unit ID configuration value is invalid.", "Invalid Unit ID configuration.");
                Application.Current.Shutdown(1);
            }

            _currentUnit = await MessageService.GetUnit(unitId);
        }
    }
}
