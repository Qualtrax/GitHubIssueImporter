using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nustache.Core;

namespace GitHubIssueImporter
{
    class Program
    {
        private const String onTimeConnectionString = "Server=ccs-db-2;Database=OnTime;Integrated Security=SSPI;";
        private const String repositoryUrl = "https://api.github.com/repos/qualtrax/testrepo";
        private const String accessToken = "dcabdcaad7cfc5da9b19e71f12d7598052a0b007";

        static void Main(string[] args)
        {
            var defectProjector = new OnTimeDefectProjector(onTimeConnectionString);
            var requestFactory = new GitHubWebRequestFactory(repositoryUrl, accessToken);
            var defectId = 0;

            while (true)
            {
                if (Int32.TryParse(Console.ReadLine(), out defectId) == false)
                    continue;

                var defect = defectProjector.Get(defectId);

                var createIssueRequest = requestFactory.Create("/issues", new
                {
                    title = Render.FileToString(@"Templates\Issue.Title.template", defect),
                    body = Render.FileToString(@"Templates\Issue.Body.template", defect)
                });

                createIssueRequest.GetResponse();
            }
        }
    }
}
