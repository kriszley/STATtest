using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NlpTaskQueue
{
    public class DocumentTaskEntity : TableEntity
    {
        public DocumentTaskEntity(string document)
        {
            this.PartitionKey = document;
            this.RowKey = document;
            this.DocumentId = document;
            this.GroupId = "";
            this.State = "";
            this.InputFiles = new List<BatchBlobUrlInput>();
            this.ResultFiles = new List<BatchBlobUrlInput>();
            this.Ts = "";
        }

        public DocumentTaskEntity()
        {
            
        }

        public string DocumentId { get; set; } // guid
        public string GroupId { get; set; } // guid
        public string State { get; set; } // uninitialized / enqueued / failed / completed / cancelled
        public string CallbackUrl { get; set; }
        [IgnoreProperty] public List<UrlUploadFile> UploadFiles {
            set
            {
                UploadFilesJson = JsonConvert.SerializeObject(value);
            }
            get
            {
                if (UploadFilesJson == null)
                {
                    return new List<UrlUploadFile>();
                }
                else
                {
                    return JsonConvert.DeserializeObject<List<UrlUploadFile>>(UploadFilesJson);
                }
            }
        }
        public string UploadFilesJson { get; set; }
        [IgnoreProperty] public List<BatchBlobUrlInput> InputFiles
        {
            set
            {
                InputFilesJson = JsonConvert.SerializeObject(value);
            }
            get
            {
                if (InputFilesJson == null)
                {
                    return new List<BatchBlobUrlInput>();
                }
                else
                {
                    return JsonConvert.DeserializeObject<List<BatchBlobUrlInput>>(InputFilesJson);
                }
            }
        } // Blob store URL
        public string InputFilesJson { get; set; }
        [IgnoreProperty] public List<BatchBlobUrlInput> ResultFiles
        {
            set
            {
                ResultFilesJson = JsonConvert.SerializeObject(value);
            }
            get
            {
                if (ResultFilesJson == null)
                {
                    return new List<BatchBlobUrlInput>();
                }
                else
                {
                    return JsonConvert.DeserializeObject<List<BatchBlobUrlInput>>(ResultFilesJson);
                }
            }
        } // Blob store URL
        public string ResultFilesJson { get; set; }
        public string Ts { get; set; }

    }
}
