using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMegaAPI.Models;

namespace ZeroMegaAPI.Interfaces
{
    interface IPosition
    {
        Task<ThingPosition> GetThingPosition(int accountId, string thingId, string datetime);

        Task<IEnumerable<ThingPosition>> GetThingPositions(int accountId, string thingId);

        Task<IEnumerable<ThingPosition>> GetThingPositions(int accountId, string thingId, DateTime lowerLimit);

        Task<IEnumerable<ThingPosition>> GetThingPositions(int accountId, string thingId, DateTime lowerLimit, DateTime upperLimit);

        Task<IEnumerable<ThingPosition>> GetAllThingsPositions(int accountId);
    }
}