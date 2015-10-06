using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;

namespace HockeyApp.Model
{
    internal static class ModelExtensions
    {
        public static async Task SaveToStorageAsync(this FeedbackMessage @this)
        {
            var helper = HockeyClient.Current.AsInternal().PlatformHelper;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(FeedbackMessage));
            
            using(var stream = new MemoryStream()) {
                serializer.WriteObject(stream, @this);
                stream.Seek(0,SeekOrigin.Begin);
                await helper.WriteStreamToFileAsync(stream, ConstantsUniversal.OpenFeedbackMessageFile, ConstantsUniversal.FeedbackAttachmentTmpDir).ConfigureAwait(false);
            }
        }

    }
}
