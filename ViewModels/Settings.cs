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
