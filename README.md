GitHub Issue Importer
===================

Application for importing OnTime items into GitHub.

To use add a configuration file called Sensitive.config with the following values

```
<?xml version="1.0" encoding="utf-8" ?>
<appSettings>
  <add key="OnTime" value="{OnTime Connection String}" />
  <add key="GitHubRepositoryApiUrl" value="{Base Repository Api Url}" />
  <add key="GitHubAccessToken" value="{GitHub API Access Token}" />
  <add key="FeatureLabels" value="{Comma Seperated Labels}" />
  <add key="DefectLabels" value="{Comma Seperated Labels}" />
</appSettings>
```
