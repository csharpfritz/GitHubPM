using Newtonsoft.Json.Linq;
using Octokit;
using StreamDeckLib;
using StreamDeckLib.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GitHubPM
{
	[ActionUuid(Uuid = "com.csharpfritz.github.issues")]
	public class MyPluginAction : BaseStreamDeckActionWithSettingsModel<Models.GitHubIssuesModel>
	{


		public override async Task OnKeyUp(StreamDeckEventPayload args)
		{


			await Manager.OpenUrlAsync(args.context, SettingsModel.ProjectUrl + "/issues");


			//update settings
			await Manager.SetSettingsAsync(args.context, SettingsModel);

		}

		public override async Task OnDidReceiveSettings(StreamDeckEventPayload args)
		{
			await base.OnDidReceiveSettings(args);

			ScrubGitHubUrl(SettingsModel.ProjectUrl);
			await GetIssueCountAsync();

			await Manager.SetTitleAsync(args.context, SettingsModel.ProjectName + "\n" + $"Issues: {SettingsModel.IssueCount}");
		}

		private async Task GetIssueCountAsync()
		{
			var client = new GitHubClient(new ProductHeaderValue("github-streamdeck-plugin"));

			var orgAndRepo = SettingsModel.ProjectUrl.Replace("https://github.com/", "");
			var issues = await client.Issue.GetAllForRepository(orgAndRepo.Split('/')[0], orgAndRepo.Split('/')[1], new RepositoryIssueRequest {
				State = ItemStateFilter.Open
			}, new ApiOptions {
				PageSize=1000
			});

			SettingsModel.IssueCount = issues.Count;
			SettingsModel.ProjectName = orgAndRepo.Split('/')[1];

		}

		private void ScrubGitHubUrl(string projectUrl)
		{
			
			// todo: text cleanup of projectUrl


		}

		public override async Task OnWillAppear(StreamDeckEventPayload args)
		{
			await base.OnWillAppear(args);

			await GetIssueCountAsync();

			await Manager.SetTitleAsync(args.context, SettingsModel.ProjectName + "\n" + $"Bugs: {SettingsModel.IssueCount}");
		}

	}
}
