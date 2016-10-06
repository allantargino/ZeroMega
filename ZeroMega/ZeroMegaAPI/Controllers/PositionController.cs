using Microsoft.Azure.ActiveDirectory.GraphClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using ZeroMegaAPI.Models;
using ZeroMegaAPI.Repositories;

namespace ZeroMegaAPI.Controllers
{
    [Authorize]
    public class PositionController : ApiController
    {
        private int _account;
        private PositionRepository _repository = new PositionRepository();

        //api/Position/
        [Route("api/Position")]
        public async Task<IEnumerable<ThingPosition>> Get()
        {
            var user = getUPN();
            _account = GetAccountId(user);

            return await _repository.GetAllThingsPositions(_account);
        }

        //api/Position/thingId/
        [Route("api/Position/{thingId}")]
        public async Task<IEnumerable<ThingPosition>> Get(string thingId)
        {
            var user = getUPN();
            _account = GetAccountId(user);

            return await _repository.GetThingPositions(_account, thingId);
        }

        //api/Position/thingId/datetime
        [Route("api/Position/{thingId}/{datetime}")]
        public async Task<ThingPosition> Get(string thingId, string datetime)
        {
            var user = getUPN();
            _account = GetAccountId(user);

            return await _repository.GetThingPosition(_account, thingId, datetime);
        }

        //api/Position/thingId?lowerLimit=yyyy-MM-dd
        public async Task<IEnumerable<ThingPosition>> Get(string thingId, DateTime lowerLimit)
        {
            var user = getUPN();
            _account = GetAccountId(user);

            return await _repository.GetThingPositions(_account, thingId, lowerLimit);
        }

        //api/Position/thingId?lowerLimit=yyyy-MM-dd&upperLimit=yyyy-MM-dd
        public async Task<IEnumerable<ThingPosition>> Get(string thingId, DateTime lowerLimit, DateTime upperLimit)
        {
            var user = getUPN();
            _account = GetAccountId(user);

            return await _repository.GetThingPositions(_account, thingId, lowerLimit, upperLimit);
        }


        #region Identity

        private string getUPN()
        {
            var identity = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identity.Claims;
            var user = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Name).Value;
            return user;
        }

        private int GetAccountId(string UserPrincipalName)
        {
            ActiveDirectoryClient activeDirectoryClient;
            activeDirectoryClient = AuthenticationHelper.GetActiveDirectoryClientAsApplication();

            string searchString = UserPrincipalName;
            User retrievedUser = new User();
            List<IUser> retrievedUsers = null;
            try
            {
                retrievedUsers = activeDirectoryClient.Users
                    .Where(user => user.UserPrincipalName.Equals(searchString))
                    .ExecuteAsync().Result.CurrentPage.ToList();
            }
            catch (Exception)
            {
                throw;
            }

            retrievedUser = (User)retrievedUsers.First();


            Application appObject = new Application();
            List<IApplication> retrievedApps = null;

            retrievedApps = activeDirectoryClient.Applications
                .Where(app => app.AppId.Equals(Constants.ClientId))
                .ExecuteAsync().Result.CurrentPage.ToList();

            appObject = (Application)retrievedApps.First();


            object value = null;
            try
            {
                if (retrievedUser != null && retrievedUser.ObjectId != null)
                {
                    IReadOnlyDictionary<string, object> extendedProperties = retrievedUser.GetExtendedProperties();
                    value = extendedProperties[Constants.extensionAccountNumber];
                }
            }
            catch (Exception)
            {
                throw;
            }

            return int.Parse(value.ToString());
        }

        #endregion

    }
}