// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId()
            };

        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource( "API" )
                {
                    ApiSecrets = {
                        new Secret( "secret_for_the_api".Sha256() )
                    }
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
                {
                    new Client
                    {
                        ClientId = "ConsoleApp_ClientId",
                        ClientSecrets = {
                            new Secret( "secret_for_the_consoleapp".Sha256() )
                            },
                            //http://docs.identityserver.io/en/latest/topics/grant_types.html
                        AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                        AllowedScopes = {
                            "API"
                        },
                    }
                };
        }

        public static List<TestUser> GetTestUsers()
        {
            return new List<TestUser>()
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "chris",
                    Password = "123456".Sha256()
                }
            };
        }
    }
}