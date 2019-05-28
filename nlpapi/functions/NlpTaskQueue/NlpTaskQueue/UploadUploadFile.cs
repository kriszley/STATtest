using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NlpTaskQueue
{
    class UploadUploadFile : UploadFile
    {
        Guid Uid { get; set; }

        public UploadUploadFile(string part, Guid uid)
        {
            Part = part;
            Uid = uid;
        }
    }
}
