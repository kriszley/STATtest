using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Common;
using Microsoft.Azure.Batch.Conventions.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NlpTaskQueue
{
    class BatchInterface
    {
        private BatchClient batchClient;
        public BatchInterface(BatchClient client)
        {
            batchClient = client;
        }

        public async Task CreateTask(Guid guid, List<BatchBlobUrlInput> inputFiles, List<BatchBlobUrlInput> outputFiles)
        {
            CloudJob job = await batchClient.JobOperations.GetJobAsync("NlpBatchProc");
            CloudTask task = new CloudTask("task_" + guid.ToString(), "cp /tmp/inputBlob.txt /tmp/outputBlob.txt");

            task.ResourceFiles = new List<ResourceFile>();
            foreach (BatchBlobUrlInput x in inputFiles) {
                ResourceFile file = new ResourceFile(x.Url, "/tmp/" + x.Part, "0644");
                task.ResourceFiles.Add(file);
            }
            task.OutputFiles = new List<OutputFile>();
            foreach (BatchBlobUrlInput x in outputFiles)
            {
                string outputContainerUrl = x.Url;
                OutputFileDestination fileDest = new OutputFileDestination(new OutputFileBlobContainerDestination(outputContainerUrl));
                OutputFile file = new OutputFile("/tmp/" + x.Part, fileDest, new OutputFileUploadOptions(OutputFileUploadCondition.TaskCompletion));
            }

            await job.AddTaskAsync(task);
        }

        public async Task<CloudTask> GetTask(Guid guid)
        {
            CloudJob job = await batchClient.JobOperations.GetJobAsync("NlpBatchProc");
            CloudTask task = job.GetTask("task_" + guid.ToString());
            return task;
        }

        public async Task<string> GetTaskState(Guid guid)
        {
            CloudTask task = await GetTask(guid);
            return task.State.ToString();
        }

        public async Task<bool> GetTaskFailed(Guid guid)
        {
            CloudTask task = await GetTask(guid);
            return task.ExecutionInformation.Result == TaskExecutionResult.Failure; 
        }

        public async void GetTaskResult(Guid guid)
        {
            CloudTask task = await GetTask(guid);
            TaskOutputStorage output = task.OutputStorage(DatabaseConnection.account);
        }

        /*
            CloudBlobClient blobClient = DatabaseConnection.blobClient;
            BatchClient batchClient = DatabaseConnection.batchClient;

            CloudBlobContainer inputContainer = DatabaseConnection.blobClient.GetContainerReference("taskinput");
            CloudBlobContainer outputContainer = DatabaseConnection.blobClient.GetContainerReference("taskoutput");
            inputContainer.CreateIfNotExists();
            outputContainer.CreateIfNotExists();

            CloudBlockBlob inputBlob = inputContainer.GetBlockBlobReference("inputBlob.txt");

            SharedAccessBlobPolicy readPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24)
            };

            SharedAccessBlobPolicy writePolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Add | SharedAccessBlobPermissions.Create,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24)
            };

            string inputBlobUrl = inputBlob.Uri + inputBlob.GetSharedAccessSignature(readPolicy);
            string outputContainerUrl = outputContainer.Uri.AbsoluteUri + outputContainer.GetSharedAccessSignature(writePolicy);

            string taskId = "foo-" + Guid.NewGuid().ToString();

            CloudJob job = await batchClient.JobOperations.GetJobAsync("NlpBatchJob");
            CloudTask task = new CloudTask(taskId, "cp /tmp/inputBlob.txt /tmp/outputBlob.txt");

            task.ResourceFiles = new List<ResourceFile> { new ResourceFile(inputBlobUrl, "/tmp/inputBlob.txt", "0644") };
            task.OutputFiles = new List<OutputFile> { new OutputFile("/tmp/outputBlob.txt", new OutputFileDestination(new OutputFileBlobContainerDestination(outputContainerUrl)), new OutputFileUploadOptions(OutputFileUploadCondition.TaskCompletion)) };
            await job.AddTaskAsync(task);
            //batchClient.PoolOperations.GetPool("foo")
            */
    }
}
