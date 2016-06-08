// <copyright file="PersistenceTransmitter.cs" company="Microsoft">
// Copyright © Microsoft. All Rights Reserved.
// </copyright>

namespace Microsoft.HockeyApp.Channel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility.Implementation;
    using Extensibility.Implementation.Tracing;
    using Services;

    /// <summary>
    /// Implements throttled and persisted transmission of telemetry to Application Insights. 
    /// </summary>
    internal class PersistenceTransmitter : IDisposable
    {
        /// <summary>
        /// A list of senders that sends transmissions. 
        /// </summary>
        private List<Sender> senders = new List<Sender>();
        
        /// <summary>
        /// The storage that is used to persist all the transmissions. 
        /// </summary>
        private BaseStorageService storage;

        /// <summary>
        /// The number of times this object was disposed.
        /// </summary>
        private int disposeCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceTransmitter"/> class.
        /// </summary>
        /// <param name="storage">The transmissions storage.</param>
        /// <param name="sendersCount">The number of senders to create.</param>
        /// <param name="createSenders">A boolean value that indicates if this class should try and create senders. This is a workaround for unit tests purposes only.</param>
        internal PersistenceTransmitter(BaseStorageService storage, int sendersCount, bool createSenders = true)
        {
            this.storage = storage;
            if (createSenders)
            {
                for (int i = 0; i < sendersCount; i++)
                {
                    this.senders.Add(new Sender(this.storage, this));
                }
            }
        }

        /// <summary>
        /// Gets a unique folder name. This folder will be used to store the transmission files.
        /// </summary>
        internal string StorageUniqueFolder
        { 
            get
            {
                return this.storage.FolderName;
            }
        }

        /// <summary>
        /// Gets or sets the interval between each successful sending. 
        /// </summary>
        internal TimeSpan? SendingInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref this.disposeCount) == 1)
            {
                this.StopSenders();
            }
        }

        /// <summary>
        /// Sending the item to the endpoint immediately without persistence.
        /// </summary>
        /// <param name="item">Telemetry item.</param>
        /// <param name="endpointAddress">Server endpoint address.</param>
        internal void SendForDeveloperMode(ITelemetry item, string endpointAddress)
        {
            try
            {
                byte[] data = JsonSerializer.Serialize(item);
                var transmission = new Transmission(new Uri(endpointAddress), data, "application/x-json-stream", JsonSerializer.CompressionType);

                transmission.SendAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                CoreEventSource.Log.LogVerbose("Failed sending event in developer mode Exception:" + exception);
            }
        }

        /// <summary>
        /// Stops the senders.  
        /// </summary>
        /// <remarks>As long as there is no Start implementation, this method should only be called from Dispose.</remarks>
        private void StopSenders()
        {
            if (this.senders == null)
            {
                return;
            }

            var stoppedTasks = new List<Task>();
            foreach (var sender in this.senders)
            {
                stoppedTasks.Add(sender.StopAsync());
            }

            Task.WaitAll(stoppedTasks.ToArray());
        }
    }
}
