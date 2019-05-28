using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NlpTaskQueue
{
    class DocumentTaskDatabase
    {
        private CloudTableClient tableClient;
        private CloudTable documentTaskTable;
        private CloudTable documentTaskGroupTable;

        public DocumentTaskDatabase(CloudTableClient tableClient)
        {
            this.tableClient = tableClient;
            documentTaskTable = tableClient.GetTableReference("documentTaskTable");
            documentTaskGroupTable = tableClient.GetTableReference("documentTaskGroupTable");
            documentTaskTable.CreateIfNotExists();
            documentTaskGroupTable.CreateIfNotExists();
        }

        // Add a new document to the database prior to creating its task in Batch
        // pass null for group if no group should be assigned, not ""
        // returns the Guid to query the task with
        public async Task<Guid> AddNewDocument(string group, string callback, List<UrlUploadFile> uploadFiles, List<BatchBlobUrlInput> inputFiles)
        {
            Guid document = Guid.NewGuid();
            DocumentTaskEntity documentTask = new DocumentTaskEntity(document.ToString());
            //documentTask.Timestamp = DateTime.UtcNow;
            documentTask.CallbackUrl = callback;
            documentTask.Ts = XmlConvert.ToString(DateTime.UtcNow, XmlDateTimeSerializationMode.Utc);
            if (uploadFiles.Count > 0)
            {
                documentTask.State = "uploading";
            }
            else
            {
                documentTask.State = "creating";
            }
            documentTask.UploadFiles = uploadFiles;
            documentTask.InputFiles = inputFiles;
            documentTask.ResultFiles = new List<BatchBlobUrlInput>();

            DocumentTaskGroupEntity documentTaskGroup = null;

            if (group != null)
            {
                documentTaskGroup = new DocumentTaskGroupEntity(document.ToString(), group);
                TableOperation taskGroupTableOp = TableOperation.Insert(documentTaskGroup);
                TableResult taskGroupTableResult = await documentTaskGroupTable.ExecuteAsync(taskGroupTableOp);
            }


            TableOperation taskTableOp = TableOperation.Insert(documentTask);
            TableResult taskTableResult = await documentTaskTable.ExecuteAsync(taskTableOp);

            return document;
        }

        public async Task<DocumentTaskEntity> GetTask(Guid guid)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<DocumentTaskEntity>(guid.ToString(), guid.ToString());
            TableResult taskTableResult = await documentTaskTable.ExecuteAsync(retrieveOperation);
            if (taskTableResult.HttpStatusCode != 200)
            {
                throw new InvalidOperationException("not_found");
            }
            DocumentTaskEntity document = (DocumentTaskEntity)taskTableResult.Result;

            return document;
        }

        public async Task UpdateTask(DocumentTaskEntity document)
        {
            TableOperation taskTableOp = TableOperation.Replace(document);
            TableResult taskTableResult = await documentTaskTable.ExecuteAsync(taskTableOp);
        }

        public async Task CancelDocument(Guid guid)
        {
            DocumentTaskEntity documentTask;
            try
            {
                documentTask = await GetTask(guid);
            }
            catch
            {
                throw new InvalidOperationException("not_found");
            }

            if (documentTask.State == "uploading" || documentTask.State == "creating" || documentTask.State == "enqueued" || documentTask.State == "processing")
            {
                documentTask.State = "cancelling";
                try
                {
                    await UpdateTask(documentTask);
                }
                catch
                {
                    throw new InvalidOperationException("update_failed");
                }
            }
            else
            {
                throw new InvalidOperationException("wrong_state");
            }
        }

        public async Task<List<DocumentTaskEntity>> ListGroup(string group)
        {
            TableQuery<DocumentTaskGroupEntity> query = new TableQuery<DocumentTaskGroupEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, group));
            List<DocumentTaskGroupEntity> result = documentTaskGroupTable.ExecuteQuery(query).ToList();
            if (result == null)
            {
                throw new InvalidOperationException("table lookup failed");
            }
            List<DocumentTaskEntity> list = new List<DocumentTaskEntity>();
            foreach (DocumentTaskGroupEntity x in result)
            {
                Guid g = new Guid(x.DocumentId);
                try
                {
                    DocumentTaskEntity document = await GetTask(g);
                    list.Add(document);
                }
                catch
                {

                }
            }
            return list;
        }

        public List<DocumentTaskEntity> ListTasks()
        {
            TableQuery<DocumentTaskEntity> query = new TableQuery<DocumentTaskEntity>();
            List<DocumentTaskEntity> result = documentTaskTable.ExecuteQuery(query).ToList();

            if (result == null)
            {
                throw new InvalidOperationException("table lookup failed");
            }

            return result;
        }
    }
}
