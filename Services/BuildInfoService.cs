using System;
using System.Reflection;

namespace SportsBettingTracker.Services
{
    public static class BuildInfoService
    {
        public static string GetBuildVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var buildTime = System.IO.File.GetLastWriteTime(assembly.Location);
            return $"Build {buildTime:yyyy.MM.dd HH:mm}";
        }
    }
}
