using System;
using System.Collections.Generic;

namespace GitHubIssueImporter
{
    public abstract class Item
    {
        public Int32 Id;
        public String Title;
        public String Description;
        public IEnumerable<RelatedItem> RelatedItems;
        public IEnumerable<Comment> Comments;
        public String Abbreviation;

        public abstract String Url { get; }
    }

    public enum ItemType
    {
        Defect = 0,
        Feature = 1,
        Task = 2,
        Incident = 3
    }

    public class Defect : Item
    {
        public String ReproductionSteps;
        public String FoundIn;
        public IEnumerable<String> Categories;

        public override String Url
        {
            get { return String.Format(LinkTemplateConstants.Defect, Id); }
        }

        public Defect()
        {
            Abbreviation = "D";
        }
    }

    public class Feature : Item
    {
        public IEnumerable<String> Categories;

        public override String Url
        {
            get { return String.Format(LinkTemplateConstants.Feature, Id); }
        }

        public Feature()
        {
            Abbreviation = "F";
        }
    }

    public class Task : Item 
    {
        public override String Url
        {
            get { return String.Format(LinkTemplateConstants.Task, Id); }
        }

        public Task()
        {
            Abbreviation = "T";
        }
    }

    public class RelatedItem
    {
        public Int32 Id;
        public Int32 Type;

        public String TypeAppreviation()
        {
            switch ((ItemType)Type)
            {
                case ItemType.Defect: return "D";
                case ItemType.Feature: return "F";
                case ItemType.Incident: return "i";
                case ItemType.Task: return "T";
            }

            return String.Empty;
        }

        public String Url()
        {
            switch ((ItemType)Type)
            {
                case ItemType.Defect: return String.Format(LinkTemplateConstants.Defect, Id);
                case ItemType.Feature: return String.Format(LinkTemplateConstants.Feature, Id);
                case ItemType.Incident: return String.Empty;
                case ItemType.Task: return String.Format(LinkTemplateConstants.Task, Id);
            }

            return String.Empty;
        }
    }

    public class Comment
    {
        public String Commenter;
        public String Text;
        public DateTime Timestamp;
    }
}
