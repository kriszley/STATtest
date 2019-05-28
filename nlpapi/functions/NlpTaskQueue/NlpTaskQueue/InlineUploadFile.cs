using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NlpTaskQueue
{
    class InlineUploadFile : UploadFile
    {
        public string Body { get; set; }

        public InlineUploadFile(string part, string body)
        {
            Part = part;
            Body = body;
        }
    }
}
