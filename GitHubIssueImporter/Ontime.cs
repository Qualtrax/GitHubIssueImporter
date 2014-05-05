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
    }

    public enum ItemType
    {
        Defect = 0,
        Feature = 1,
        Incident = 3
    }

    public class Defect : Item
    {
        public String ReproductionSteps;

        public Defect()
        {
            Abbreviation = "D";
        }
    }

    public class Feature : Item
    {
        public Feature()
        {
            Abbreviation = "F";
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
