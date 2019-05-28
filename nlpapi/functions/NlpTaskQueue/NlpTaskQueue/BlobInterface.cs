using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NlpTaskQueue
{
    class BlobInterface
    {
        private CloudTableClient TableClient;
        private CloudBlobClient BlobClient;

        public BlobInterface(CloudTableClient tableClient, CloudBlobClient blobClient)
        {
            TableClient = tableClient;
            BlobClient = blobClient;
        }

        public static bool IsUrlBlobStorage(string url)
        {
            return false;
        }

        public string GetSasUrlForUpload(Guid uploadId)
        {
            return "";
        }

        public async Task<Guid> UploadInlineFile(string body)
        {
            Guid guid = Guid.NewGuid();
            CloudBlobContainer container = DatabaseConnection.blobClient.GetContainerReference("taskinput");
            CloudBlockBlob blob = container.GetBlockBlobReference(guid.ToString());
            byte[] arr = Encoding.UTF8.GetBytes(body);
            await blob.UploadFromByteArrayAsync(arr, 0, arr.Length);

            return guid;
        }

        public string GetFileSasReadUrl(Guid guid)
        {
            CloudBlobContainer inputContainer = DatabaseConnection.blobClient.GetContainerReference("taskinput");
            inputContainer.CreateIfNotExists();

            CloudBlockBlob inputBlob = inputContainer.GetBlockBlobReference(guid.ToString());

            SharedAccessBlobPolicy readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24)
            };

            string inputBlobUrl = inputBlob.Uri + inputBlob.GetSharedAccessSignature(readPolicy);
            return inputBlobUrl;
        }

        public string GetFileSasWriteUrl()
        {
            CloudBlobContainer outputContainer = DatabaseConnection.blobClient.GetContainerReference("taskoutput");
            outputContainer.CreateIfNotExists();

            SharedAccessBlobPolicy writePolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Add | SharedAccessBlobPermissions.Create,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24)
            };

            string outputContainerUrl = outputContainer.Uri.AbsoluteUri + outputContainer.GetSharedAccessSignature(writePolicy);
            return outputContainerUrl;
        }
    }
}