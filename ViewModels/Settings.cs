using Microsoft.Extensions.DependencyInjection;
using VkNet;
using VkNet.AudioBypassService.Extensions;

namespace VkLikerMVVM
{
    class Settings : JsonSettings
    {
        private static Settings _instance;
        public static Settings Instance
        {
            get => _instance ??= new Settings();
        }

        public string Token { get; set; }
    }
}
