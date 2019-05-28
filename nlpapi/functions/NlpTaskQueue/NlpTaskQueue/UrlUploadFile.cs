using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NlpTaskQueue
{
    public class UrlUploadFile : UploadFile
    {
        public string Url { get; set; }

        public UrlUploadFile(string part, string url)
        {
            Part = part;
            Url = url;
        }
    }
}
