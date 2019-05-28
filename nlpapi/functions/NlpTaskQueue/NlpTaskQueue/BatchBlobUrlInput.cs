using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NlpTaskQueue
{
    public class BatchBlobUrlInput
    {
        public string Part { get; set; }
        public string Url { get; set; }

        public BatchBlobUrlInput()
        {

        }

        public BatchBlobUrlInput(string part, string url)
        {
            Part = part;
            Url = url;
        }
        public BatchBlobUrlInput(UrlUploadFile file)
        {
            Part = file.Part;
            Url = file.Url;
        }
    }
}
