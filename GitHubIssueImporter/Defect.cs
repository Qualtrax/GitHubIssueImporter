using System;
using System.Collections.Generic;

namespace GitHubIssueImporter
{
    public class Defect
    {
        public Int32 Id;
        public String Title;
        public String Description;
        public String ReproductionSteps;
        public IEnumerable<RelatedItem> RelatedItems;
        public IEnumerable<Comment> Comments;
    }
}
