using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceNowTeamFoundationSync.Service
{
    partial class ServiceNowTeamFoundationSyncService : ServiceBase
    {
        private ServiceProcessor serviceProcessor;
        private Thread thread;
        public ServiceNowTeamFoundationSyncService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                serviceProcessor = new ServiceProcessor();
                thread = new Thread(new ThreadStart(serviceProcessor.SynchronizationTask));
                serviceProcessor.CurrentThread = this.thread;
                serviceProcessor.CurrentThread.Start();
            }
            catch { }
        }

        protected override void OnStop()
        {
            try
            {
                if (serviceProcessor != null)
                {
                    //Send a shutdown command to the processor
                    serviceProcessor.ShutDown();
                    //Wait for the thread to finish
                    serviceProcessor.CurrentThread.Join();
                }
            }
            catch { }
        }
    }
}
