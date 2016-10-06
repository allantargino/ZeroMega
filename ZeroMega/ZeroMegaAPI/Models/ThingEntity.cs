using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZeroMegaAPI.Models
{
    public class ThingEntity : TableEntity
    {
        // Your entity type must expose a parameter-less constructor
        public ThingEntity() { }

        // Define the PK and RK
        public ThingEntity(string id_thing, string datetime_event)
        {
            this.PartitionKey = id_thing;
            this.RowKey = datetime_event;
        }


        public string id_thing { get; set; }

        public DateTime datetime_event { get; set; }

        public string account { get; set; }
    
        public string date_event { get; set; }

        public string cgi { get; set; }

        public string latitude { get; set; }

        public string longitude { get; set; }

        public string numa { get; set; }

        public string numb { get; set; }

        public string time_event { get; set; }
    }
}