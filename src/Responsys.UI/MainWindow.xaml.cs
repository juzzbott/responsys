using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Microsoft.Practices.Unity.Configuration;
using Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Addresses.Interface;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Responsys.UI.Services;
using System.IO;

namespace Enivate.ResponseHub.Responsys.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
	{

		/// <summary>
		/// The timer for checking for messages
		/// </summary>
		private DispatcherTimer _msgServiceTimer;

		/// <summary>
		/// The timer to update the visual timer on screen
		/// </summary>
		private DispatcherTimer _jobTimer;

		/// <summary>
		/// The timer interval key.
		/// </summary>
		private string _serviceIntervalKey = "ServiceTimerInterval";

		/// <summary>
		/// The current unit.
		/// </summary>
		private Unit _currentUnit;

		/// <summary>
		/// The current job message
		/// </summary>
		private JobMessage _currentJobMessageDisplayed;

        private bool _enablePrinting;
        private bool _databaseExists;
        private bool _initialLoad = true;

		protected ILogger Logger;

        protected ResponseHubApiService MessageService;
        protected PrintedMessageRecordService PrintRecordService;

        public MainWindow()
        {
            // Unity configuration loader
            UnityConfiguration.Container = new UnityContainer().LoadConfiguration();

            Logger = ServiceLocator.Get<ILogger>();

            MessageService = new ResponseHubApiService();
            PrintRecordService = new PrintedMessageRecordService();

            // Check if printing is enabled
            _databaseExists = CheckDatabaseExists();
            _enablePrinting = _databaseExists;

            InitializeComponent();

            Focus();

        }

        private async void _msgServiceTimer_Tick(object sender, EventArgs e)
        {
            await GetLatestJobs();

        }

        private async Task GetLatestJobs()
        {

            bool jobsExist = false;

            // Submit the messages
            IList<JobMessage> latestList = await MessageService.GetLatestMessages(_currentUnit.Id);
            latestList = latestList.OrderBy(i => i.Timestamp).ToList();

            IList<JobMessage> notPrintedJobs = new List<JobMessage>();
            IList<JobMessage> validJobs = new List<JobMessage>();

            // Loop through the jobs, skip any that start with RE:, as it's not actually a job
            foreach (JobMessage message in latestList.Where(i => i.Type == MessageType.Job))
            {

                // Determine that jobs exist
                jobsExist = true;

                if (Regex.IsMatch(message.MessageContent, "^(RE:|[A-Za-z0-9]*\\sRE:)\\s.*$", RegexOptions.IgnoreCase))
                {
                    continue;
                }

                validJobs.Add(message);

                if (PrintRecordService.PrintRecordExistsForJob(message.Id))
                {
                    continue;
                }

                notPrintedJobs.Add(message);
            }
            
            // If there are no jobs, so the no jobs message
            if (!jobsExist)
            {

                GrdSystemLoading.Visibility = Visibility.Collapsed;
                GrdNoJob.Visibility = Visibility.Visible;
                GrdJobDetails.Visibility = Visibility.Collapsed;
            }

            // If there are jobs that aren't yet printed, then display them and print them
            if (notPrintedJobs.Count > 0)
            {
                foreach(JobMessage message in notPrintedJobs)
                {
                    UpdateJobMessageDisplay(message, !_initialLoad);

                    // If the database exist, just create the printed records for now
                    if (_databaseExists)
                    {
                        PrintRecordService.CreatePrintRecord(message.Id, DateTime.UtcNow);
                    }
                }

                // Set the database as exists
                _databaseExists = true;

            }
            else
            {
                // No jobs need printing, so just display the most recent job
                JobMessage latestMessage = validJobs.OrderByDescending(i => i.Timestamp).FirstOrDefault();
                UpdateJobMessageDisplay(latestMessage, false);
            }

            // We've done the initial load, so flag that as false
            _initialLoad = false;

            // If the msg service timer is not started, then start it
            if (!_msgServiceTimer.IsEnabled)
            {
                _msgServiceTimer.Start();
            }

            // Load the last 5 messages. We actually get 6, and remove any duplicates
            StkLatestJobs.Children.Clear();
            foreach (JobMessage message in latestList.Where(i => i.Id != _currentJobMessageDisplayed.Id).OrderByDescending(i => i.Timestamp).Take(5))
            {
                StkLatestJobs.Children.Add(new TextBlock()
                {
                    Text = message.MessageContent,
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    Style = (Style)FindResource("LatestJob")
                });
            }
        }

        private void UpdateJobMessageDisplay(JobMessage job, bool printJob)
        {

            // Update the job elapsed
            UpdateJobElapsedTime(job);

            GrdSystemLoading.Visibility = Visibility.Collapsed;
            GrdNoJob.Visibility = Visibility.Collapsed;
            GrdJobDetails.Visibility = Visibility.Visible;

            LblJobNumber.Content = job.JobNumber;
            LblJobNumber.Style = (job.Capcodes.FirstOrDefault(i => i.Capcode == _currentUnit.Capcode).Priority == MessagePriority.Emergency ? (Style)FindResource("JobNumberLabelEmergency") : (Style)FindResource("JobNumberLabel"));

            if (job.Location != null)
            {
                LblGridReference.Content = job.Location.MapReference;
                LblAddress.Content = job.Location.Address.FormattedAddress;
            }
            else
            {
                LblGridReference.Content = "";
                LblAddress.Content = "";
            }
            LblMessage.Text = job.MessageContent;

            // Download the image from mapbox
            string imageFilename = GetMapImageFilename(job);

            if (!String.IsNullOrEmpty(imageFilename))
            {
                ImgMap.Source = new BitmapImage(new Uri(imageFilename));
            }

            // Only try to print if the service timer is running
            if (printJob)
            {
                PrintCurrentJob(job, imageFilename);
            }

            // Initialise the message timer
            _currentJobMessageDisplayed = job;
            _jobTimer = new DispatcherTimer();
            _jobTimer.Interval = TimeSpan.FromMilliseconds(500);
            _jobTimer.Tick += _jobTimer_Tick;
            _jobTimer.Start();
        }

        private string GetMapImageFilename(JobMessage job)
        {
            string imageFilename = "";
            if (job.Location != null && job.Location.Coordinates != null && !job.Location.Coordinates.IsEmpty())
            {
                imageFilename = MediaService.MapBoxStaticImage(job.Location.Coordinates.Latitude, job.Location.Coordinates.Longitude, 15, "550x550");
            }

            return imageFilename;
        }

        private void PrintCurrentJob(JobMessage job, string imageFilename)
        {

            // If printing is disabled, just exit
            if (!_enablePrinting)
            {
                return;
            }

            PrintService printSvc = new PrintService();
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
            printSvc.PrintJobReport(job, imageFilename, styles, PrintRecordService, _currentUnit, Logger);
        }

        private void _jobTimer_Tick(object sender, EventArgs e)
		{
			// if there is no current job, just exit
			if (_currentJobMessageDisplayed == null)
			{
				return;
			}

			// Update the job elapsed
			UpdateJobElapsedTime(_currentJobMessageDisplayed);

		}

		private void UpdateJobElapsedTime(JobMessage job)
		{

			// Get the timespan between now and job timestamp
			TimeSpan elapsedTime = DateTime.Now - job.Timestamp.ToLocalTime();

			// Set the timer value
			if (elapsedTime.TotalHours < 1)
			{
				LblTimer.Content = String.Format("{0}:{1}", elapsedTime.Minutes.ToString("D2"), elapsedTime.Seconds.ToString("D2"));
			}
			else
			{
				LblTimer.Content = String.Format("{0}:{1}:{2}", (int)elapsedTime.TotalHours, elapsedTime.Minutes.ToString("D2"), elapsedTime.Seconds.ToString("D2"));
			}

			if (job.Capcodes.FirstOrDefault(i => i.Capcode == _currentUnit.Capcode).Priority == MessagePriority.Emergency)
			{
				// Set the styles
				if (elapsedTime.TotalMinutes < 5)
				{
					GrdTimer.Style = (Style)FindResource("TimerGridOk");
				}
				else if (elapsedTime.TotalMinutes < 8)
				{
					GrdTimer.Style = (Style)FindResource("TimerGridWarning");
				}
				else
				{
					GrdTimer.Style = (Style)FindResource("TimerGridDanger");
				}
			}
			else
			{
				if (elapsedTime.TotalMinutes < 10)
				{
					GrdTimer.Style = (Style)FindResource("TimerGridOk");
				}
				else
				{
					GrdTimer.Style = (Style)FindResource("TimerGridWarning");
				}
			}

		}
                
        private void FullscreenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (WindowStyle == WindowStyle.SingleBorderWindow)
            {
                // Fullscreen
                Visibility = Visibility.Collapsed;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.NoResize;
                MnuFullScreen.IsChecked = true;
                ShowInTaskbar = false;
                Visibility = Visibility.Visible;
            }
            else
            {
                // Windowed
                Visibility = Visibility.Collapsed;
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.CanResizeWithGrip;
                MnuFullScreen.IsChecked = false;
                ShowInTaskbar = true;
                Visibility = Visibility.Visible;
            }
        }

        private void AlwaysOnTopCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Topmost == true)
            {
                // not top most
                Topmost = false;
                MnuAlwaysOnTop.IsChecked = false;
            }
            else
            {
                // Top most
                Topmost = true;
                MnuAlwaysOnTop.IsChecked = true;
            }
        }

        private void TestJobReportCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TestJobReport testJobReportWindow = new TestJobReport();
            testJobReportWindow.ShowDialog();
        }

        private void ExitCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void PrintJobReportCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Download the image from mapbox
            string imageFilename = GetMapImageFilename(_currentJobMessageDisplayed);
            PrintCurrentJob(_currentJobMessageDisplayed, imageFilename);
        }

        private void EnablePrintingCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Set the enable printing flag
            _enablePrinting = !_enablePrinting;
            MnuEnablePrinting.IsChecked = _enablePrinting;
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {

            // Set the printing enabled flag
            MnuEnablePrinting.IsChecked = _enablePrinting;

            int timerInterval = 10000;

            // If there is no interval setting, then log warning
            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[_serviceIntervalKey]))
            {
                await Logger.Warn("The configuration setting 'ServiceTimerInterval' is not present in the configuration. Defaulting to 10 seconds.");
            }
            else
            {
                // Get the timer interval
                Int32.TryParse(ConfigurationManager.AppSettings[_serviceIntervalKey], out timerInterval);
            }

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
            
            // Initialise the message timer
            _msgServiceTimer = new DispatcherTimer();
            _msgServiceTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            _msgServiceTimer.Tick += _msgServiceTimer_Tick;

            // Get latest jobs on load
            await GetLatestJobs();
        }

        private bool CheckDatabaseExists()
        {
            LiteDB.ConnectionString connectionString = new LiteDB.ConnectionString(ConfigurationManager.ConnectionStrings["LiteDB"].ConnectionString);
            string filename = connectionString.Filename;
            return File.Exists(filename);
        }
    }
}
