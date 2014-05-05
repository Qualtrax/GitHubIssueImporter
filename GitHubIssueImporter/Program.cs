using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nustache.Core;

namespace GitHubIssueImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var itemProjector = new OnTimeProjector(ConfigurationManager.AppSettings["OnTime"]);
            var requestFactory = new GitHubWebRequestFactory(
                ConfigurationManager.AppSettings["GitHubRepositoryApiUrl"],
                ConfigurationManager.AppSettings["GitHubAccessToken"]);

            var defectLabels = ConfigurationManager.AppSettings["DefectLabels"].Split(',');
            var featureLabels = ConfigurationManager.AppSettings["FeatureLabels"].Split(',');

            var itemId = 0;

            while (true)
            {
                Console.Write("\nEnter a work item number ie. D1591: ");

                var input = Console.ReadLine();

                if (input.Length < 2 || Char.IsLetter(input.First()) == false)
                    continue;

                var itemType = Parse(input.First());

                if (Int32.TryParse(input.Substring(1), out itemId) == false)
                    continue;

                Item item = null;
                String[] labels = null;

                if (itemType == ItemType.Defect)
                {
                    item = itemProjector.GetDefect(itemId);
                    labels = defectLabels;
                }
                else if (itemType == ItemType.Feature)
                {
                    item = itemProjector.GetFeature(itemId);
                    labels = featureLabels;
                }
                else if (itemType == ItemType.Incident)
                {
                    throw new NotImplementedException("Incidents are not yet supported");
                }

                var createIssueRequest = requestFactory.Create("/issues", new
                {
                    title = Render.FileToString(@"Templates\Issue.Title.template", item),
                    body = Render.FileToString(@"Templates\Issue.Body.template", item),
                    labels = labels
                });

                createIssueRequest.GetResponse().Dispose();
            }
        }


        private static ItemType Parse(Char itemTypeAbbreviation)
        {
            switch (itemTypeAbbreviation)
            {
                case 'D':
                case 'd': return ItemType.Defect;
                case 'F':
                case 'f': return ItemType.Feature;
                case 'I':
                case 'i': return ItemType.Incident;
            }

            throw new ArgumentOutOfRangeException("itemTypeAbbreviation");
        }
    }
}
