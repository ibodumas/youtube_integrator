using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using System.IO;
using System.Text;

namespace HadoopMapReduceYoutubeData
{
    class Program
    {
        private static YouTubeService ytService = Auth();

        static void Main(string[] args)
        {
            string[] countryCodes = new[] {"US", "IN", "CN", "GB", "DE", "NG"};
            StringBuilder sbuilBuilder = new StringBuilder();
            sbuilBuilder.Append("CategoryID" + "\t" + "ChannelID" + "\t" + "ViewCount" + "\t" + "CountryCode" + "\t" + "Title" + "\n");

            foreach (var country in countryCodes)
            {
                var searchListRequest = ytService.Search.List("snippet");
                searchListRequest.MaxResults = 50;
                searchListRequest.RegionCode = country;

                var searchListResponse = searchListRequest.Execute();

                foreach (var searchListResponseElement in searchListResponse.Items)
                {
                    if (searchListResponseElement.Id != null && searchListResponseElement.Id.VideoId != null)
                    {
                        //Fetching information for a given video ID
                        var videoRequest = ytService.Videos.List("snippet,contentDetails,localizations,statistics,status");
                        videoRequest.Id = searchListResponseElement.Id.VideoId;
                        var singleReponse = videoRequest.Execute();
                        foreach (var obj in singleReponse.Items)
                        {
                            sbuilBuilder.Append(obj.Snippet.CategoryId + "\t" + obj.Snippet.ChannelId + "\t" + obj.Statistics.ViewCount + "\t"
                                + country + "\t" + obj.Snippet.Title + "\n");
                        }
                    }
                }
            }

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", "");
            File.WriteAllText(appDirectory + "Results.txt", sbuilBuilder.ToString());
        }


        private static YouTubeService Auth()
        {
            UserCredential creds;
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", "");
            using (var stream = new FileStream(appDirectory + "YouTube_Client.json",
            FileMode.Open, FileAccess.Read))
            {
                creds = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                new[] { YouTubeService.Scope.YoutubeReadonly },
                "user", System.Threading.CancellationToken.None, new
                FileDataStore("YouTubeKey")).Result;
            }

            var service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = creds,
                ApplicationName = "YouTubeKey"
            });

            return service;
        }
    }
}
