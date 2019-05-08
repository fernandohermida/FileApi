using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinccubeApi.Model
{
    public class FileModel
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string MimeType { get; set; }
    }
}
