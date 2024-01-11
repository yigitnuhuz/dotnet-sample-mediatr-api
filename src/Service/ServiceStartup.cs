using Data;

namespace Service
{
    public static class ServiceStartup
    {
        public static void ConfigureService()
        {
            DataStartup.Configure();
        }
    }
}