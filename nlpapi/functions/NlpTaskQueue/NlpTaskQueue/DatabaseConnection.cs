using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NlpTaskQueue
{
    static class DatabaseConnection
    {
        public static string storageConnectionString = System.Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        public static CloudStorageAccount account = CloudStorageAccount.Parse(storageConnectionString);
        public static CloudTableClient tableClient = account.CreateCloudTableClient();
        public static CloudBlobClient blobClient = account.CreateCloudBlobClient();
        public static CloudQueueClient queueClient = account.CreateCloudQueueClient();

        public static DocumentTaskDatabase db = new DocumentTaskDatabase(tableClient);
        public static BlobInterface blx = new BlobInterface(tableClient, blobClient);

        // TODO: move this elsewhere
        const string BatchAccountName = "nlpbatchproc";
        const string BatchAccountKey = "ORuNvQNOztMeHlSmZ82FbXFcvLTEjGEN30pOU0Q2+/jc+5IquPwNeqpf/CmbIVfM9cw29c7OerUPz69AhB5Amw==";
        const string BatchAccountUrl = "https://nlpbatchproc.canadacentral.batch.azure.com";

        public static BatchClient batchClient = BatchClient.Open(new BatchSharedKeyCredentials(BatchAccountUrl, BatchAccountName, BatchAccountKey));
        public static BatchInterface bx = new BatchInterface(batchClient);

    }
}
