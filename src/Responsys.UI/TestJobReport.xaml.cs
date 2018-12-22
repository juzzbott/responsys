using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.DataAccess;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Responsys.UI.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.Windows.Shapes;

namespace Enivate.ResponseHub.Responsys.UI
{
    /// <summary>
    /// Interaction logic for TestJobReport.xaml
    /// </summary>
    public partial class TestJobReport : Window
    {

        private Unit _currentUnit;

        protected IJobMessageService JobMessageService;
        protected IUnitService UnitService;

        public TestJobReport()
        {

            JobMessageService = ServiceLocator.Get<IJobMessageService>();
            UnitService = ServiceLocator.Get<IUnitService>();

            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            JobMessage job = null;

            await LoadUnitDetails();

            IList<JobMessage> latestList = await JobMessageService.GetMostRecent(new List<string>() { _currentUnit.Capcode }, 10, 0);

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


            //FlowDocument flowDoc = flowScroller.Document; //(FlowDocument)Application.LoadComponent(new Uri("Assets/PrintedJobTemplate.xaml", UriKind.Relative));
            FlowDocument flowDoc = (FlowDocument)Application.LoadComponent(new Uri("Assets/PrintedJobTemplate.xaml", UriKind.Relative));
            Section sctnHeader = new Section();
            sctnHeader.BorderThickness = new Thickness(0);

            Paragraph pghJobNumber = new Paragraph(new Run(job.JobNumber));
            pghJobNumber.Style = (job.Priority == MessagePriority.Emergency ? (Style)FindResource("ReportJobNumberEmergency") : (Style)FindResource("ReportJobNumber"));
            sctnHeader.Blocks.Add(pghJobNumber);

            Paragraph pghTimestamp = new Paragraph(new Run(job.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss")));
            pghTimestamp.Style = (Style)FindResource("ReportTimestamp");
            sctnHeader.Blocks.Add(pghTimestamp);

            Paragraph pghMapReference = new Paragraph(new Run(job.Location.MapReference));
            pghMapReference.Style = (Style)FindResource("ReportMapReference");
            sctnHeader.Blocks.Add(pghMapReference);

            Paragraph pghAddress = new Paragraph(new Run(job.Location.Address.FormattedAddress));
            pghAddress.Style = (Style)FindResource("ReportAddress");
            sctnHeader.Blocks.Add(pghAddress);

            Image imgMap = new Image();
            imgMap.Source = new BitmapImage(new Uri("/Enivate.ResponseHub;component/Assets/test-map-image.png", UriKind.Relative));
            imgMap.Style = (Style)FindResource("ReportMapImage");
            InlineUIContainer imgContainer = new InlineUIContainer(imgMap);
            Paragraph pgMapImage = new Paragraph(imgContainer);
            pgMapImage.Style = (Style)FindResource("ReportMapImageContainer");
            sctnHeader.Blocks.Add(pgMapImage);

            Paragraph pghMessageContent = new Paragraph(new Run(job.MessageContent));
            pghMessageContent.Style = (Style)FindResource("ReportMessageContent");
            sctnHeader.Blocks.Add(pghMessageContent);

            flowDoc.Blocks.InsertBefore(flowDoc.Blocks.FirstBlock, sctnHeader);

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

            _currentUnit = await UnitService.GetById(unitId);
        }
    }
}
