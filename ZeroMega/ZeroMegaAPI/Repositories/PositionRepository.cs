using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ZeroMegaAPI.Interfaces;
using ZeroMegaAPI.Models;

namespace ZeroMegaAPI.Repositories
{
    public class PositionRepository:IPosition
    {
        CloudTable _table = null;

        public PositionRepository()
        {
            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create a table client for interacting with the table service 
            _table = tableClient.GetTableReference("xDRTable");
        }


        public async Task<IEnumerable<ThingPosition>> GetAllThingsPositions(int accountId)
        {
            var thingsPositions = new List<ThingPosition>();
            TableContinuationToken token = null;
            try
            {
                do
                {
                    TableQuerySegment<ThingEntity> segment = await _table.ExecuteQuerySegmentedAsync(new TableQuery<ThingEntity>()
                    {
                        FilterString = "account eq '" + accountId + "'"
                    }, token);

                    token = segment.ContinuationToken;
                    var result = from thing
                                 in segment
                                 select new ThingPosition()
                                 {
                                     IDThing = thing.id_thing,
                                     DateTimeEvent = thing.datetime_event,
                                     Latitude = thing.latitude,
                                     Longitude = thing.longitude,
                                 };
                    thingsPositions.AddRange(result);
                }
                while (token != null);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return thingsPositions;
        }

        public async Task<ThingPosition> GetThingPosition(int accountId, string thingId, string datetime)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<ThingEntity>(thingId, datetime);
            TableResult result = await _table.ExecuteAsync(retrieveOperation);
            ThingEntity thing = result.Result as ThingEntity;

            return new ThingPosition()
            {
                IDThing = thing.id_thing,
                DateTimeEvent = thing.datetime_event,
                Latitude = thing.latitude,
                Longitude = thing.longitude,
            };
        }

        public async Task<IEnumerable<ThingPosition>> GetThingPositions(int accountId, string thingId)
        {
            var thingsPositions = new List<ThingPosition>();
            TableContinuationToken token = null;
            try
            {
                do
                {
                    TableQuerySegment<ThingEntity> segment = await _table.ExecuteQuerySegmentedAsync(new TableQuery<ThingEntity>()
                    {
                        FilterString = "account eq '" + accountId + "' and id_thing eq '" + thingId + "'"
                    }, token);

                    token = segment.ContinuationToken;
                    var result = from thing
                                 in segment
                                 select new ThingPosition()
                                 {
                                     IDThing = thing.id_thing,
                                     DateTimeEvent = thing.datetime_event,
                                     Latitude = thing.latitude,
                                     Longitude = thing.longitude,
                                 };
                    thingsPositions.AddRange(result);
                }
                while (token != null);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return thingsPositions;
        }

        public async Task<IEnumerable<ThingPosition>> GetThingPositions(int accountId, string thingId, DateTime lowerLimit)
        {
            var thingsPositions = new List<ThingPosition>();
            TableContinuationToken token = null;
            try
            {
                do
                {
                    TableQuerySegment<ThingEntity> segment = await _table.ExecuteQuerySegmentedAsync(new TableQuery<ThingEntity>()
                    {
                        FilterString = "(account eq '" + accountId + "') and (id_thing eq '" + thingId + "') and (Timestamp ge datetime'" + lowerLimit.ToString("yyyy-MM-dd") + "')"
                    }, token);

                    token = segment.ContinuationToken;
                    var result = from thing
                                 in segment
                                 select new ThingPosition()
                                 {
                                     IDThing = thing.id_thing,
                                     DateTimeEvent = thing.datetime_event,
                                     Latitude = thing.latitude,
                                     Longitude = thing.longitude,
                                 };
                    thingsPositions.AddRange(result);
                }
                while (token != null);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return thingsPositions;
        }

        public async Task<IEnumerable<ThingPosition>> GetThingPositions(int accountId, string thingId, DateTime lowerLimit, DateTime upperLimit)
        {
            var thingsPositions = new List<ThingPosition>();
            TableContinuationToken token = null;
            try
            {
                do
                {
                    TableQuerySegment<ThingEntity> segment = await _table.ExecuteQuerySegmentedAsync(new TableQuery<ThingEntity>()
                    {
                        FilterString = "(account eq '" + accountId + "') and (id_thing eq '" + thingId + "') and (Timestamp ge datetime'" + lowerLimit.ToString("yyyy-MM-dd") + "') and (Timestamp le datetime'" + upperLimit.ToString("yyyy-MM-dd") + "')"
                    }, token);

                    token = segment.ContinuationToken;
                    var result = from thing
                                 in segment
                                 select new ThingPosition()
                                 {
                                     IDThing = thing.id_thing,
                                     DateTimeEvent = thing.datetime_event,
                                     Latitude = thing.latitude,
                                     Longitude = thing.longitude,
                                 };
                    thingsPositions.AddRange(result);
                }
                while (token != null);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return thingsPositions;
        }
    }
}