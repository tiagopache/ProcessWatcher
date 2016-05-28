using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Management.Instrumentation;

namespace ProcessWatcher
{
    public class EventWatcherAsync
    {
        private Dictionary<int, string> processes;

        private void WmiEventHandler(object sender, EventArrivedEventArgs e)
        {
            var processId = int.Parse(((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Handle"].ToString());
            var processName = ((ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value)["Name"].ToString();

            if (!processes.ContainsKey(processId))
                processes.Add(processId, processName);
            else
            {
                processes.Remove(processId);
                Console.WriteLine("It's a match! Closing process: ");
            }

            Console.WriteLine($"TargetInstance.Handle :       {processId.ToString()}");
            Console.WriteLine($"TargetInstance.Name :         {processName}");
        }

        private void watcher(ManagementScope scope, string wmiQuery)
        {
            
            ManagementEventWatcher watcher;

            watcher = new ManagementEventWatcher(scope, new EventQuery(wmiQuery));

            watcher.EventArrived += new EventArrivedEventHandler(this.WmiEventHandler);

            watcher.Start();
            Console.Read();
            watcher.Stop();
        }

        public EventWatcherAsync()
        {
            try
            {
                processes = new Dictionary<int, string>();

                string computerName = Environment.MachineName;
                ManagementScope scope;

                scope = new ManagementScope($"\\\\{computerName}\\root\\CIMV2", null);
                scope.Connect();

                string wmiQueryCreation = "Select * From __InstanceCreationEvent Within 1 Where TargetInstance ISA 'Win32_Process' ";
                string wmiQueryDeletion = "Select * From __InstanceDeletionEvent Within 1 Where TargetInstance ISA 'Win32_Process' ";

                Task.Run(() => watcher(scope, wmiQueryCreation));
                Task.Run(() => watcher(scope, wmiQueryDeletion));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception {e.Message} Trace {e.StackTrace}");
            }
        }
    }

    class Program
    {
        

        static void Main(string[] args)
        {
            Console.WriteLine("Listening process creation and deletion, Press enter to exit");
            var eventWatcher = new EventWatcherAsync();
            Console.Read();
        }
    }
}
