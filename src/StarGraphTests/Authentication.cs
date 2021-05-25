using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarGraphTests
{
    public static class Authentication
    {
        public static string GetGitHubAccessToken()
        {
            var appConfig = new ConfigurationBuilder().AddUserSecrets<GitHubApiTests>().Build();
            string token = appConfig["github-token-starchart-readuser"];
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("token not found");
            return token;
        }
    }
}
