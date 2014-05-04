using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubIssueImporter
{
    public class RelatedItem
    {
        public Int32 Id;
        public Int32 Type;
        
        public String TypeAppreviation()
        {
            switch (Type)
            {
                case 0: return "D";
                case 1: return "F";
                case 3: return "i";
            }

            return String.Empty;
        }
    }
}
