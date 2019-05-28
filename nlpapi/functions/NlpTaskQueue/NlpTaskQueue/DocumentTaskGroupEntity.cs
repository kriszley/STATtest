using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NlpTaskQueue
{
    class DocumentTaskGroupEntity : TableEntity
    {
        public DocumentTaskGroupEntity(string document, string group)
        {
            this.PartitionKey = group;
            this.RowKey = document;
            this.DocumentId = document;
            this.GroupId = group;
        }

        public DocumentTaskGroupEntity()
        {

        }

        public string DocumentId { get; set; } // guid
        public string GroupId { get; set; } // guid        
    }
}
