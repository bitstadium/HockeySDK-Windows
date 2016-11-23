﻿// <copyright file="Storage.cs" company="Microsoft">
// Copyright © Microsoft. All Rights Reserved.
// </copyright>

namespace Microsoft.HockeyApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Extensibility.Implementation;
    using Extensibility.Implementation.Tracing;

    using Channel;
    using System.IO.IsolatedStorage;

    /// <summary>
    /// The storage service
    /// </summary>
    internal sealed class StorageService : BaseStorageService
    {
        private const string DefaultStorageFolderName = "HockeyApp\\";

        private readonly object peekLockObj = new object();
        private readonly object storageFolderLock = new object();
        private readonly FixedSizeQueue<string> deletedFilesQueue = new FixedSizeQueue<string>(10);

        private long storageSize = 0;
        private long storageCountFiles = 0;
        private bool storageFolderInitialized = false;
        private uint transmissionsDropped = 0;

        private string _storageFolderName;
        private string _storageFolder;

        private IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

        /// <summary>
        /// Initializes the <see cref="BaseStorageService" />
        /// </summary>
        /// <param name="uniqueFolderName">A folder name. Under this folder all the transmissions will be saved.</param>
        internal override void Init(string uniqueFolderName)
        {
            this.peekedTransmissions = new SnapshottingDictionary<string, string>();
            _storageFolderName = uniqueFolderName;
            if (string.IsNullOrEmpty(uniqueFolderName))
            {
                _storageFolderName = DefaultStorageFolderName;
            }

            this.CapacityInBytes = 10 * 1024 * 1024; // 10 MB
            this.MaxFiles = 5000;

            Task.Factory.StartNew(this.DeleteObsoleteFiles)
                .ContinueWith(
                    task =>
                    {
                        string msg = string.Format(CultureInfo.InvariantCulture, "Storage: Unhandled exception in DeleteObsoleteFiles: {0}", task.Exception);
                        CoreEventSource.Log.LogVerbose(msg);
                    },
                    TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Gets the storage's folder name.
        /// </summary>
        internal override string FolderName
        {
            get { return _storageFolderName; }
        }

        /// <summary>
        /// Gets the storage folder. If storage folder couldn't be created, null will be returned.
        /// </summary>        
        private string StorageFolder
        {
            get
            {
                if (!this.storageFolderInitialized)
                {
                    lock (this.storageFolderLock)
                    {
                        if (!this.storageFolderInitialized)
                        {
                            try
                            {
                                _storageFolder = _storageFolderName;
                                if (!isolatedStorage.DirectoryExists(_storageFolder))
                                {
                                    isolatedStorage.CreateDirectory(_storageFolder);
                                }
                            }
                            catch (Exception e)
                            {
                                _storageFolder = null;
                                string error = string.Format(CultureInfo.InvariantCulture, "Failed to create storage folder: {0}", e);
                                CoreEventSource.Log.LogVerbose(error);
                            }

                            this.storageFolderInitialized = true;
                            string msg = string.Format(CultureInfo.InvariantCulture, "Storage folder: {0}", _storageFolder == null ? "null" : _storageFolderName);
                            CoreEventSource.Log.LogVerbose(msg);
                        }
                    }
                }

                return _storageFolder;
            }
        }

        /// <summary>
        /// Reads an item from the storage. Order is Last-In-First-Out. 
        /// When the Transmission is no longer needed (it was either sent or failed with a non-retriable error) it should be disposed. 
        /// </summary>
        internal override StorageTransmission Peek()
        {
            var files = this.GetFiles("*.trn", top: 50);

            lock (this.peekLockObj)
            {
                foreach (var file in files)
                {
                    try
                    {
                        // if a file was peeked before, skip it (wait until it is disposed).  
                        if (this.peekedTransmissions.ContainsKey(file) == false && this.deletedFilesQueue.Contains(file) == false)
                        {
                            // Load the transmission from disk.
                            StorageTransmission storageTransmissionItem = LoadTransmissionFromFileAsync(file).ConfigureAwait(false).GetAwaiter().GetResult();

                            // when item is disposed it should be removed from the peeked list.
                            storageTransmissionItem.Disposing = item => this.OnPeekedItemDisposed(file);

                            // add the transmission to the list.
                            this.peekedTransmissions.Add(file, storageTransmissionItem.FullFilePath);
                            return storageTransmissionItem;
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = string.Format(CultureInfo.InvariantCulture, "Failed to load an item from the storage. file: {0} Exception: {1}", file, e);
                        CoreEventSource.Log.LogVerbose(msg);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal override void Delete(StorageTransmission item)
        {
            try
            {
                if (this.StorageFolder == null)
                {
                    return;
                }

                // Initial storage size calculation. 
                CalculateSizeAsync();

                long fileSize = GetSizeAsync(item.FileName);
                isolatedStorage.DeleteFile(Path.Combine(StorageFolder, item.FileName));

                this.deletedFilesQueue.Enqueue(item.FileName);

                // calculate size                
                Interlocked.Add(ref this.storageSize, -fileSize);
                Interlocked.Decrement(ref this.storageCountFiles);
            }
            catch (IOException e)
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "Failed to delete a file. file: {0} Exception: {1}", item == null ? "null" : item.FullFilePath, e);
                CoreEventSource.Log.LogVerbose(msg);
            }
        }

        /// <summary>
        /// Enqueues the asynchronous.
        /// </summary>
        /// <param name="transmission">The transmission.</param>
        /// <returns>A task</returns>
        internal override async Task EnqueueAsync(Transmission transmission)
        {
            try
            {
                if (transmission == null || this.StorageFolder == null)
                {
                    return;
                }

                // Initial storage size calculation. 
                CalculateSizeAsync();

                if ((ulong)this.storageSize >= this.CapacityInBytes || this.storageCountFiles >= this.MaxFiles)
                {
                    // if max storage capacity has reached, drop the transmission (but log every 100 lost transmissions). 
                    if (this.transmissionsDropped++ % 100 == 0)
                    {
                        CoreEventSource.Log.LogVerbose("Total transmissions dropped: " + this.transmissionsDropped);
                    }

                    return;
                }

                // Writes content to a temporaty file and only then rename to avoid the Peek from reading the file before it is being written.
                // Creates the temp file name
                string tempFileName = Guid.NewGuid().ToString("N");

                // Now that the file got created we can increase the files count
                Interlocked.Increment(ref this.storageCountFiles);

                // Saves transmission to the temp file
                await SaveTransmissionToFileAsync(transmission, tempFileName).ConfigureAwait(false);

                // Now that the file is written increase storage size. 
                long temporaryFileSize = this.GetSizeAsync(tempFileName);//.ConfigureAwait(false);
                Interlocked.Add(ref this.storageSize, temporaryFileSize);

                // Creates a new file name
                string now = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string newFileName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}.trn", now, tempFileName);

                // Renames the file
                isolatedStorage.MoveFile(Path.Combine(StorageFolder, tempFileName), Path.Combine(StorageFolder, newFileName));
            }
            catch (Exception e)
            {
                CoreEventSource.Log.LogVerbose(string.Format(CultureInfo.InvariantCulture, "EnqueueAsync: Exception: {0}", e));
            }
        }

        /// <summary>
        /// Saves the transmission to file asynchronous.
        /// </summary>
        /// <param name="transmission">The transmission.</param>
        /// <param name="file">The file.</param>
        /// <returns>A task</returns>
        private async Task SaveTransmissionToFileAsync(Transmission transmission, string file)
        {
            try
            {
                using (Stream stream = isolatedStorage.OpenFile(Path.Combine(StorageFolder, file), FileMode.Create, FileAccess.Write))
                {
                    await StorageTransmission.SaveAsync(transmission, stream).ConfigureAwait(false);
                }
            }
            catch (UnauthorizedAccessException)
            {
                string message = string.Format("Failed to save transmission to file. UnauthorizedAccessException. File path: {0}, FileName: {1}", StorageFolder, file);
                CoreEventSource.Log.LogVerbose(message);
                throw;
            }
        }

        /// <summary>
        /// Loads the transmission from file asynchronous.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>The transmission</returns>
        private async Task<StorageTransmission> LoadTransmissionFromFileAsync(string file)
        {
            try
            {
                using (Stream stream = isolatedStorage.OpenFile(Path.Combine(StorageFolder, file), FileMode.Open, FileAccess.Read))
                {
                    StorageTransmission storageTransmissionItem = await StorageTransmission.CreateFromStreamAsync(stream, file).ConfigureAwait(false);
                    return storageTransmissionItem;
                }
            }
            catch (Exception e)
            {
                string message = string.Format("Failed to load transmission from file. File path: {0}, FileName: {1}, Exception: {2}", StorageFolder, file, e);
                CoreEventSource.Log.LogVerbose(message);
                throw;
            }
        }

        /// <summary>
        /// Get files from <see cref="storageFolder"/>.
        /// </summary>
        /// <param name="fileQuery">Define the logic for sorting the files.</param>
        /// <param name="filterByExtension">Defines a file extension. This method will return only files with this extension.</param>
        /// <param name="top">Define how many files to return. This can be useful when the directory has a lot of files, in that case 
        /// GetFilesAsync will have a performance hit.</param>
        private IEnumerable<string> GetFiles(string filterByExtension, int top)
        {
            try
            {
                if (this.StorageFolder != null)
                {
                    return isolatedStorage.GetFileNames(StorageFolder + @"\" + filterByExtension).Take(top);
                }
            }
            catch (Exception e)
            {
                string msg = string.Format(CultureInfo.InvariantCulture, "Peek failed while get files from storage. Exception: " + e);
                CoreEventSource.Log.LogVerbose(msg);
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets a file's size.
        /// </summary>
        private long GetSizeAsync(string file)
        {
            using (var stream = isolatedStorage.OpenFile(Path.Combine(StorageFolder, file), FileMode.Open, FileAccess.Read))
            {
                return stream.Length;
            }
        }

        /// <summary>
        /// Check the storage limits and return true if they reached. 
        /// Storage limits are defined by the number of files and the total size on disk. 
        /// </summary>        
        private void CalculateSizeAsync()
        {
            var storageFiles = isolatedStorage.GetFileNames(StorageFolder + @"\*.*");

            this.storageCountFiles = (long)storageFiles.Count();

            long storageSizeInBytes = 0;
            foreach (var file in storageFiles)
            {
                storageSizeInBytes += GetSizeAsync(file);
            }

            this.storageSize = storageSizeInBytes;
        }

        /// <summary>
        /// Enqueue is saving a transmission to a <c>tmp</c> file and after a successful write operation it renames it to a <c>trn</c> file. 
        /// A file without a <c>trn</c> extension is ignored by Storage.Peek(), so if a process is taken down before rename happens 
        /// it will stay on the disk forever. 
        /// This thread deletes files with the <c>tmp</c> extension that exists on disk for more than 5 minutes.
        /// </summary>
        private void DeleteObsoleteFiles()
        {
            try
            {
                var files = this.GetFiles("*.tmp", 50);
                foreach (var file in files)
                {
                    var creationTime = isolatedStorage.GetCreationTime(Path.Combine(StorageFolder, file));
                    // if the file is older then a minute - delete it.
                    if (DateTime.Now - creationTime.UtcDateTime >= TimeSpan.FromMinutes(5))
                    {
                        isolatedStorage.DeleteFile(Path.Combine(StorageFolder, file));
                    }
                }
            }
            catch (Exception e)
            {
                CoreEventSource.Log.LogVerbose("Failed to delete tmp files. Exception: " + e);
            }
        }
    }
}

