using System;
using System.Collections.Generic;
using System.IO;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp
{
    class CustomFileObject
    {
        public CustomFileObject() { }
        public CustomFileObject(string filePath)
        {
            FilePath = filePath;
        }
        public string FilePath
        {
            set;get;
        }
        public string FileName
        {
            get
            {
                 return Path.GetFileNameWithoutExtension(FilePath); 
            }
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}
