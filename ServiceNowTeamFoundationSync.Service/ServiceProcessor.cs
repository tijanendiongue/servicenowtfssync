using ServiceNowTeamFoundationSync.BL;
using System;
using System.Configuration;
using System.Threading;

namespace ServiceNowTeamFoundationSync.Service
{
    class ServiceProcessor
    {
        // ===================  THREADING VARIABLES  ========================
        // ==================================================================
        // Variable to hold Thread on which the object is processing on,
        // allows for the proper startup and shutdown behavior for the object
        public Thread CurrentThread;
        private volatile bool shutDownThreadFlag = false;
        Synchronizer Synchronizer;
        // ==================================================================
        // ==================================================================
        private AppSettingsReader settingsReader;
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
                                                        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool ShutDownThreadFlag { get => shutDownThreadFlag; set => shutDownThreadFlag = value; }

        public ServiceProcessor()
        {
            try
            {
                //Initialize the  flag to allow the service to start
                shutDownThreadFlag = false;
                settingsReader = new AppSettingsReader();
                Synchronizer = new Synchronizer();
            }
            catch
            {

            }
        }

        /// <summary>
        /// Send a shutdown signal to the synchronization task.
        /// </summary>
        public void ShutDown()
        {
            try
            {
                shutDownThreadFlag = true;
                logger.Info("Shutting down service...");
            }
            catch(Exception ex)
            {
                logger.Error(string.Format("Error shutting service down: {0}", ex));
            }
        }

        /// <summary>
        /// Task that run the synchronization process between TFS and ServiceNow every x minutes until stopped.
        /// </summary>
        public void SynchronizationTask()
        {
            logger.Info(@"Starting Synchronization task...");
            int spinTimeInterval = 30; // Default sync intervalle is 30 minutes
            spinTimeInterval = (int)settingsReader.GetValue("ProcessTimeInterval", typeof(int));
            bool processAtStart = (bool)settingsReader.GetValue("ProcessAtStart", typeof(bool));
            logger.Info(string.Format(@"Synchronization Interval: {0}", spinTimeInterval));
            logger.Info(string.Format(@"Process at Start: {0}", processAtStart.ToString().ToUpper()));

            var currentDateTime = DateTime.Now;
            var nextTime = currentDateTime.AddMinutes(spinTimeInterval);
            while (!shutDownThreadFlag)
            {
                bool pulse = false;
                
                try
                {
                    
                    currentDateTime = DateTime.Now;
                    if ((nextTime <= currentDateTime) | processAtStart)
                    {
                        pulse = true;
                        processAtStart = false;
                        nextTime = currentDateTime.AddMinutes(spinTimeInterval);
                    }
                    // It is time to run syncrhonization?
                    if (pulse)
                    {
                        logger.Info("Starting synchronization run");                        
                        try
                        {
                            Synchronizer.UpdateTeamFoundation();
                            Synchronizer.UpdateServiceNow();
                        }
                        catch (Exception ex)
                        {
                            logger.Error(string.Format(@"Synchronization error: {0}", ex.ToString()));
                        }
                        finally
                        {
                            logger.Info("Completed synchronization run");
                            logger.Info(string.Format("Next run at {0}", nextTime.ToString()));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }


        private void SleepIntermittently(int totalTime)
        {
            int sleptTime = 0;
            const int intermittentSleepIncrement = 100; // Small amount to sleep for responsiveness

            // Wake up every 100 milli-seconds to check if we need
            // to stop or not.  More efficient then sleeping for
            // TotalTime which blocks Shutdown for however long
            // TotalTime is configured for.
            while (!shutDownThreadFlag && sleptTime < totalTime)
            {
                Thread.Sleep(intermittentSleepIncrement);
                sleptTime += intermittentSleepIncrement;
            }
        }
    }
}
