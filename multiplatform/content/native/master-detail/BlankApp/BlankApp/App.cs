using System;

namespace BlankApp
{
    public class App
    {
        #if (CreateBackendProject)
		public static bool UseMockDataStore = true;
		public static string AzureMobileAppUrl = "https://[CONFIGURE-THIS-URL].azurewebsites.net";
		#endif		

        public static void Initialize()
        {
            #if (CreateBackendProject)
            if (UseMockDataStore)
				ServiceLocator.Instance.Register<IDataStore<Item>, MockDataStore>();
			else
                ServiceLocator.Instance.Register<IDataStore<Item>, AzureDataStore>();
            #else
            ServiceLocator.Instance.Register<IDataStore<Item>, MockDataStore>();
            #endif
        }
    }
}
