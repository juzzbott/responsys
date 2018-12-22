using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Enivate.ResponseHub.Responsys.UI.Commands
{
    public static class ApplicationCommands
    {

        public static RoutedUICommand FullscreenCommand = new RoutedUICommand("Fullscreen", "Fullscreen", typeof(ApplicationCommands));

        public static RoutedUICommand AlwaysOnTopCommand = new RoutedUICommand("Always On Top", "AlwaysOnTop", typeof(ApplicationCommands));

        public static RoutedUICommand TestJobReportCommand = new RoutedUICommand("Test Job Report", "TestJobReport", typeof(ApplicationCommands));

        public static RoutedUICommand PrintJobReportCommand = new RoutedUICommand("Print Job Report", "PrintJobReport", typeof(ApplicationCommands));

        public static RoutedUICommand ExitCommand = new RoutedUICommand("Exit", "Exit", typeof(ApplicationCommands));

    }
}
