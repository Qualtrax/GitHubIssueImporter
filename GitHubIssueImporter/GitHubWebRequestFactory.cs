﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace GitHubIssueImporter
{
    public class GitHubWebRequestFactory
    {
        private String repositoryUri;
        private String accessToken;
        private JavaScriptSerializer serializer;

        public GitHubWebRequestFactory(String repositoryUri, String accessToken)
        {
            this.repositoryUri = repositoryUri;
            this.accessToken = accessToken;
            serializer = new JavaScriptSerializer();
        }

        public HttpWebRequest Create(String path, dynamic obj)
        {
            var request = WebRequest.Create(BuildUrl(path)) as HttpWebRequest;
            request.Method = "POST";
            request.Accept = "application/vnd.github.v3+json";
            request.UserAgent = "GitHub-Issue-Importer";

            var json = serializer.Serialize(obj);

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(json);
                writer.Flush();
            }

            return request;
        }

        private String BuildUrl(String path)
        {
            return repositoryUri + path + "?access_token=" + accessToken;
        }
    }
}
