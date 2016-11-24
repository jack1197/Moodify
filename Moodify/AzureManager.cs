using Microsoft.WindowsAzure.MobileServices;
using Moodify.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify
{
    class AzureManager
    {


        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<Timeline> timelineTable;


        private AzureManager()
        {
            this.client = new MobileServiceClient("jw-msatest.azurewebsites.net");
            this.timelineTable = this.client.GetTable<Timeline>();
        }


        public MobileServiceClient AzureClient
        {
            get { return client;  }
        }


        public async Task<List<Timeline>> GetTimelines()
        {
            return await this.timelineTable.ToListAsync();
        }


        public static AzureManager AzureManagerInstance
        {
            get
            {
                if(instance == null)
                {
                    instance = new AzureManager();
                }
                return instance;
            }
        }
    }
}
