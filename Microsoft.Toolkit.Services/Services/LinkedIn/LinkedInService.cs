// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Toolkit.Services.Core;
#if WINRT
using Microsoft.Toolkit.Services.PlatformSpecific.Uwp;
#endif

#if NET462
using Microsoft.Toolkit.Services.PlatformSpecific.NetFramework;
#endif

namespace Microsoft.Toolkit.Services.LinkedIn
{
    /// <summary>
    /// Class for connecting to LinkedIn.
    /// </summary>
    public class LinkedInService
    {
        /// <summary>
        /// Private singleton field.
        /// </summary>
        private static LinkedInService _instance;

        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static LinkedInService Instance => _instance ?? (_instance = new LinkedInService());

        private LinkedInDataProvider _provider;

        private LinkedInOAuthTokens _oAuthTokens;

        private LinkedInPermissions _requiredPermissions;

        private IAuthenticationBroker _authenticationBroker;
        private IPasswordManager _passwordManager;
        private IStorageManager _storageManager;
        private bool _isInitialized = false;

        /// <summary>
        /// Gets a reference to an instance of the underlying data provider.
        /// </summary>
        public LinkedInDataProvider Provider => _provider ?? (_provider = new LinkedInDataProvider(_oAuthTokens, _requiredPermissions, _authenticationBroker, _passwordManager, _storageManager));

        private LinkedInService()
        {
        }

        /// <summary>
        /// Log user in to LinkedIn.
        /// </summary>
        /// <returns>Returns success or failure of login attempt.</returns>
        public Task<bool> LoginAsync()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Initialized needs to be called first.");
            }

            return Provider.LoginAsync();
        }

        /// <summary>
        /// Share content to LinkedIn.
        /// </summary>
        /// <param name="commentContainingUrl">Comment containing a Url.</param>
        /// <param name="visibilityCode">Code for who to share with.</param>
        /// <returns>Boolean indicating success or failure.</returns>
        public Task<LinkedInShareResponse> ShareActivityAsync(string commentContainingUrl, LinkedInShareVisibility visibilityCode = LinkedInShareVisibility.ConnectionsOnly)
        {
            var shareRequest = new LinkedInShareRequest
            {
                Comment = commentContainingUrl,
                Visibility = new LinkedInVisibility { Code = LinkedInVisibility.ParseVisibilityEnumToString(visibilityCode) }
            };

            return ShareActivityAsync(shareRequest);
        }

        /// <summary>
        /// Share content to LinkedIn.
        /// </summary>
        /// <param name="shareRequest">Share request.</param>
        /// <returns>Boolean indicating success or failure.</returns>
        public Task<LinkedInShareResponse> ShareActivityAsync(LinkedInShareRequest shareRequest)
        {
            return Provider.ShareDataAsync<LinkedInShareRequest, LinkedInShareResponse>(shareRequest);
        }

        /// <summary>
        /// Log user out of LinkedIn.
        /// </summary>
        [Obsolete("Logout is deprecated, please use LogoutAsync instead.", true)]
        public void Logout()
        {
            Provider.Logout();
        }

        /// <summary>
        /// Log user out of LinkedIn.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task LogoutAsync()
        {
            _isInitialized = false;
            return Provider.LogoutAsync();
        }

#if WINRT
        /// <summary>
        /// Initialize underlying provider with relevant token information for Uwp.
        /// </summary>
        /// <param name="oAuthTokens">Token instance.</param>
        /// <param name="requiredPermissions">Scope / permissions app requires user to sign up for.</param>
        /// <returns>Success or failure.</returns>
        public bool Initialize(LinkedInOAuthTokens oAuthTokens, LinkedInPermissions requiredPermissions = LinkedInPermissions.NotSet)
        {
            return Initialize(oAuthTokens, new UwpAuthenticationBroker(), new UwpPasswordManager(), new UwpStorageManager(), requiredPermissions);
        }

        /// <summary>
        /// Initialize underlying provider with relevant token information.
        /// </summary>
        /// <param name="clientId">Client Id.</param>
        /// <param name="clientSecret">Client secret.</param>
        /// <param name="callbackUri">Callback URI. Has to match callback URI defined at www.linkedin.com/developer/apps/ (can be arbitrary).</param>
        /// <returns>Success or failure.</returns>
        public bool Initialize(string clientId, string clientSecret, string callbackUri)
        {
            return Initialize(clientId, clientSecret, callbackUri, new UwpAuthenticationBroker(), new UwpPasswordManager(), new UwpStorageManager());
        }
#endif

#if NET462
        /// <summary>
        /// Initialize underlying provider with relevant token information for Uwp.
        /// </summary>
        /// <param name="oAuthTokens">Token instance.</param>
        /// <param name="requiredPermissions">Scope / permissions app requires user to sign up for.</param>
        /// <returns>Success or failure.</returns>
        public bool Initialize(LinkedInOAuthTokens oAuthTokens, LinkedInPermissions requiredPermissions = LinkedInPermissions.NotSet)
        {
            return Initialize(oAuthTokens, new NetFrameworkAuthenticationBroker(), new NetFrameworkPasswordManager(), new NetFrameworkStorageManager(), requiredPermissions);
        }

        /// <summary>
        /// Initialize underlying provider with relevant token information.
        /// </summary>
        /// <param name="clientId">Client Id.</param>
        /// <param name="clientSecret">Client secret.</param>
        /// <param name="callbackUri">Callback URI. Has to match callback URI defined at www.linkedin.com/developer/apps/ (can be arbitrary).</param>
        /// <returns>Success or failure.</returns>
        public bool Initialize(string clientId, string clientSecret, string callbackUri)
        {
            return Initialize(clientId, clientSecret, callbackUri, new NetFrameworkAuthenticationBroker(), new NetFrameworkPasswordManager(), new NetFrameworkStorageManager());
        }
#endif

        /// <summary>
        /// Initialize underlying provider with relevant token information.
        /// </summary>
        /// <param name="clientId">Client Id.</param>
        /// <param name="clientSecret">Client secret.</param>
        /// <param name="callbackUri">Callback URI. Has to match callback URI defined at www.linkedin.com/developer/apps/ (can be arbitrary).</param>
        /// <param name="authentication">Authentication result interface.</param>
        /// <param name="passwordManager">Password Manager interface, store the password.</param>
        /// <param name="storageManager">Storage Manager interface.</param>
        /// <returns>Success or failure.</returns>
        public bool Initialize(string clientId, string clientSecret, string callbackUri, IAuthenticationBroker authentication, IPasswordManager passwordManager, IStorageManager storageManager)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentNullException(nameof(clientSecret));
            }

            if (string.IsNullOrEmpty(callbackUri))
            {
                throw new ArgumentNullException(nameof(callbackUri));
            }

            var oAuthTokens = new LinkedInOAuthTokens
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                CallbackUri = callbackUri
            };

            return Initialize(oAuthTokens, authentication, passwordManager, storageManager, LinkedInPermissions.ReadBasicProfile);
        }

        /// <summary>
        /// Initialize underlying provider with relevant token information.
        /// </summary>
        /// <param name="oAuthTokens">Token instance.</param>
        /// <param name="authentication">Authentication result interface.</param>
        /// <param name="passwordManager">Password Manager interface, store the password.</param>
        /// <param name="storageManager">Storage Manager interface.</param>
        /// <param name="requiredPermissions">Scope / permissions app requires user to sign up for.</param>
        /// <returns>Success or failure.</returns>
        public bool Initialize(LinkedInOAuthTokens oAuthTokens, IAuthenticationBroker authentication, IPasswordManager passwordManager, IStorageManager storageManager, LinkedInPermissions requiredPermissions = LinkedInPermissions.NotSet)
        {
            _oAuthTokens = oAuthTokens ?? throw new ArgumentNullException(nameof(oAuthTokens));
            _authenticationBroker = authentication ?? throw new ArgumentNullException(nameof(authentication));
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _passwordManager = passwordManager ?? throw new ArgumentNullException(nameof(passwordManager));

            _requiredPermissions = requiredPermissions;

            Provider.RequiredPermissions = requiredPermissions;
            Provider.Tokens = oAuthTokens;

            _isInitialized = true;

            return true;
        }

        /// <summary>
        /// Request list data from service provider based upon a given config / query.
        /// </summary>
        /// <typeparam name="T">Strong type of model.</typeparam>
        /// <param name="config">LinkedInDataConfig instance.</param>
        /// <param name="maxRecords">Upper limit of records to return.</param>
        /// <param name="startRecord">Index of paged results.</param>
        /// <param name="fields">A comma separated string of required fields, which will have strongly typed representation in the model passed in.</param>
        /// <returns>Strongly typed list of data returned from the service.</returns>
        public async Task<List<T>> RequestAsync<T>(LinkedInDataConfig config, int maxRecords = 20, int startRecord = 0, string fields = "id")
        {
            List<T> queryResults = new List<T>();

            var results = await Provider.GetDataAsync<T>(config, maxRecords, startRecord, fields);

            foreach (var result in results)
            {
                queryResults.Add(result);
            }

            return queryResults;
        }

        /// <summary>
        /// Retrieve logged in users profile details.
        /// </summary>
        /// <param name="requireEmailAddress">Require email address - which needs user consensus.</param>
        /// <returns>Strongly typed profile.</returns>
        public async Task<LinkedInProfile> GetUserProfileAsync(bool requireEmailAddress = false)
        {
            var fields = LinkedInProfile.Fields;

            if (requireEmailAddress)
            {
                if (!_requiredPermissions.HasFlag(LinkedInPermissions.ReadEmailAddress))
                {
                    throw new InvalidOperationException("Please re-initialize with email permission and call LoginAsync again so user may grant access.");
                }

                fields += ",email-address";
            }

            if (Provider.LoggedIn)
            {
                var results = await LinkedInService.Instance.RequestAsync<LinkedInProfile>(new LinkedInDataConfig { Query = "/people" }, 1, 0, fields);

                return results[0];
            }

            var isLoggedIn = await LoginAsync();
            if (isLoggedIn)
            {
                return await GetUserProfileAsync();
            }

            return null;
        }
    }
}
