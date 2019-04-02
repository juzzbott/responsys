using System;
using System.Collections.Generic;
using System.Configuration;
using System.Printing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.Responsys.UI.Services
{
    public class PrintService
    {

        public void PrintJobReport(JobMessage job, string mapImagePath, Dictionary<string, Style> styles, PrintedMessageRecordService printRecordService, ILogger logger)
        {
            // Get the formatted flow document.
            FlowDocument flowDoc = GetFormattedDocument(job, mapImagePath, styles);

            LocalPrintServer localPrintServer = new LocalPrintServer();
            PrintQueue printQueue = localPrintServer.DefaultPrintQueue;

            // Get the print count
            int printCount = 1;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["PrintCopyCount"]))
            {
                int.TryParse(ConfigurationManager.AppSettings["PrintCopyCount"], out printCount);
            }

            // Create a XpsDocumentWriter object, open a Windows common print dialog.
            // This methods returns a ref parameter that represents information about the dimensions of the printer media.
            XpsDocumentWriter docWriter = PrintQueue.CreateXpsDocumentWriter(printQueue);
            PageImageableArea ia = printQueue.GetPrintCapabilities().PageImageableArea;
            PrintTicket pt = printQueue.UserPrintTicket;
            //pt.CopyCount = printCount;

            for (int i = 0; i < printCount; i++)
            {
                if (docWriter != null && ia != null)
                {
                    DocumentPaginator paginator = ((IDocumentPaginatorSource)flowDoc).DocumentPaginator;

                    // Change the PageSize and PagePadding for the document to match the CanvasSize for the printer device.
                    paginator.PageSize = new Size((double)ia.ExtentWidth, (double)ia.ExtentHeight);
                    Thickness pagePadding = flowDoc.PagePadding;
                    flowDoc.PagePadding = new Thickness(
                    Math.Max(ia.OriginWidth, pagePadding.Left),
                    Math.Max(ia.OriginHeight, pagePadding.Top),
                    Math.Max((double)pt.PageMediaSize.Width - (double)(ia.OriginWidth + ia.ExtentWidth), pagePadding.Right),
                    Math.Max((double)pt.PageMediaSize.Height - (double)(ia.OriginHeight + ia.ExtentHeight), pagePadding.Bottom));
                    flowDoc.ColumnWidth = double.PositiveInfinity;
                    // Send DocumentPaginator to the printer.
                    docWriter.Write(paginator);

                    // Log that the job was printed
                    logger.Info(String.Format("Job number '{0}' printed", job.JobNumber));
                }
            }

            // Add the print report
            printRecordService.CreatePrintRecord(job.Id, DateTime.UtcNow);

        }

        /// <summary>
        /// Returns the formatted flow document used to generate the document. 
        /// </summary>
        /// <param name="job">The job to format the document for.</param>
        /// <param name="mapImagePath">The path to the image file.</param>
        /// <param name="styles">The list of styles to format with the document.</param>
        /// <returns></returns>
        public static FlowDocument GetFormattedDocument(JobMessage job, string mapImagePath, Dictionary<string, Style> styles)
        {
            FlowDocument flowDoc = (FlowDocument)Application.LoadComponent(new Uri("Assets/PrintedJobTemplate.xaml", UriKind.Relative));
            Section sctnHeader = new Section
            {
                BorderThickness = new Thickness(0)
            };

            Paragraph pghJobNumber = new Paragraph(new Run(job.JobNumber))
            {
                Style = (job.Priority == MessagePriority.Emergency ? styles["ReportJobNumberEmergency"] : styles["ReportJobNumber"])
            };
            sctnHeader.Blocks.Add(pghJobNumber);

            Paragraph pghTimestamp = new Paragraph(new Run(job.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss")))
            {
                Style = styles["ReportTimestamp"]
            };
            sctnHeader.Blocks.Add(pghTimestamp);

            Span mapRef = new Span(new Bold(new Run(job.Location.MapReference)));
            mapRef.Style = styles["ReportMapReference"];
            Span address = new Span(new Run(string.Format(" - {0}", job.Location.Address.FormattedAddress)));
            address.Style = styles["ReportAddress"];
            Paragraph pghMapReference = new Paragraph()
            {
                Style = styles["ReportMapReferenceParagraph"]
            };
            pghMapReference.Inlines.Add(mapRef);
            pghMapReference.Inlines.Add(address);
            sctnHeader.Blocks.Add(pghMapReference);

            Image imgMap = new Image
            {
                Source = new BitmapImage(new Uri(mapImagePath, UriKind.Relative)),
                Style = styles["ReportMapImage"]
            };
            InlineUIContainer imgContainer = new InlineUIContainer(imgMap);
            Paragraph pgMapImage = new Paragraph(imgContainer)
            {
                Style = styles["ReportMapImageContainer"]
            };
            sctnHeader.Blocks.Add(pgMapImage);

            Paragraph pghMessageContent = new Paragraph(new Run(job.MessageContent))
            {
                Style = styles["ReportMessageContent"]
            };
            sctnHeader.Blocks.Add(pghMessageContent);

            flowDoc.Blocks.InsertBefore(flowDoc.Blocks.FirstBlock, sctnHeader);
            return flowDoc;
        }
    }
}
