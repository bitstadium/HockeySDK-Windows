using System.Threading.Tasks;
using Microsoft.HockeyApp.Channel;
using System.Collections.Generic;

namespace Microsoft.HockeyApp.Services
{
    class TestStorageService : BaseStorageService
    {
        private List<Transmission> storage = new List<Transmission>();

        internal override string FolderName
        {
            get
            {
                return "";
            }
        }

        internal override void Delete(StorageTransmission transmission)
        {
            storage.Remove(transmission);
        }

        internal override Task EnqueueAsync(Transmission transmission)
        {
            storage.Add(transmission);
            return Task.CompletedTask;
        }

        internal override void Init(string uniqueFolderName)
        {
        }

        internal override StorageTransmission Peek()
        {
            return (StorageTransmission)(storage.Count > 0 ? storage[storage.Count - 1] : null);
        }
    }
}
