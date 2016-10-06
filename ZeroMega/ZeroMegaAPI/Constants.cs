//----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
//----------------------------------------------------------------------------------

namespace ZeroMegaAPI
{
    internal class Constants
    {
        public const string TenantName = "<Your Tenant Name>.onmicrosoft.com";
        public const string TenantId = "<Your Tenant ID>";
        public const string ClientId = "<Your Client ID>";
        public const string ClientSecret = "<Your Client ID>";
        public const string AuthString = "https://login.windows.net/" + TenantName;
        public const string ResourceUrl = "https://graph.windows.net";
        public readonly string extensionAccountNumber = "extension_" + ClientId.Replace("-","") + "_AccountNumber";
    }
}