using IdentityModel;
using IdentityModel.Client;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static Stopwatch stopWatch = new Stopwatch();
        static SemaphoreSlim semaphore = new SemaphoreSlim(10);
        static int random = 600;

        static void Main(string[] args)
        {
            stopWatch.Start();

            // Do work
            Semaphore();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Total Time in Main: " + Math.Round(stopWatch.Elapsed.TotalSeconds, 2));
            Console.ReadLine();
        }

        static void Semaphore()
        {
            for (int i = 0; i < 100; i++)
            {
                
                Thread.Sleep(random);

                // Threading goes through all instantly.
                //MainAsync(i);
                var t = new Thread(() => MainAsync(i));
                t.Start();
            }
        }

        private async static void MainAsync(int id)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("{0} is Waiting", id);

            var httpClient = new HttpClient();

            semaphore.Wait(random);
            {
                Random rng = new Random();
                random = rng.Next(400, 1000);
                try
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("{0} Is Running", id);

                    Thread.Sleep(random);

                    //Just a sample call with an invalid access token.
                    // The expected response from this call is 401 Unauthorized
                    var apiResponse = await httpClient.GetAsync("http://localhost:5001/identity");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid_access_token");

                    //The API is protected, let's ask the user for credentials and exchanged them with an access token
                    if (apiResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        //Ask User
                        //Console.Write("Username:");
                        var username = "chris";
                        //Console.Write("Password:");
                        var password = "123456";

                        //Make the call and get the access token back
                        var identityServerResponse = await httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
                        {
                            Address = "http://localhost:5000/connect/token",
                            GrantType = "password",

                            ClientId = "ConsoleApp_ClientId",
                            ClientSecret = "secret_for_the_consoleapp",
                            Scope = "API",

                            UserName = username,
                            Password = password.ToSha256()
                        });

                        //all good?
                        if (!identityServerResponse.IsError)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine();
                            Console.WriteLine("SUCCESS!!");
                            Console.WriteLine();
                            Console.WriteLine("Connection number: " + id);
                            Console.WriteLine("Access Token: ");
                            Console.WriteLine(identityServerResponse.AccessToken);

                            //Call the API with the correct access token
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", identityServerResponse.AccessToken);
                            apiResponse = await httpClient.GetAsync("http://localhost:5001/identity");

                            Console.WriteLine();
                            Console.WriteLine("API response:");
                            Console.WriteLine(await apiResponse.Content.ReadAsStringAsync());
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine();
                            Console.WriteLine("Failed to login with error:");
                            Console.WriteLine(identityServerResponse.Error);
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine();
                        Console.WriteLine("YOU ARE NOT PROTECTED!!!");
                    }
                }
                finally
                {
                    semaphore.Release(1);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} Is Exiting", id);
                }
            }
        }
    }
}