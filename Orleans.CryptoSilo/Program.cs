using System;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime.Configuration;
using Orleans.Serialization.CryptoSerializer;
using System.Reflection;
using Orleans.CryptoGrain.Interfaces;

namespace Orleans.CryptoSilo
{
    /// <summary>
    /// Orleans test silo host
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            // The Orleans silo environment is initialized in its own app domain in order to more
            // closely emulate the distributed situation, when the client and the server cannot
            // pass data via shared memory.
            AppDomain hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = InitSilo,
                AppDomainInitializerArguments = args,
            });

            var config = ClientConfiguration.LocalhostSilo();
            config.SerializationProviders.Add(typeof(CryptoSerializer).GetTypeInfo());
            config.FallbackSerializationProvider = typeof(Orleans.Serialization.OrleansJsonSerializer).GetTypeInfo();
            GrainClient.Initialize(config);

            // TODO: once the previous call returns, the silo is up and running.
            //       This is the place your custom logic, for example calling client logic
            //       or initializing an HTTP front end for accepting incoming requests.

            Console.WriteLine("Orleans Silo is running.\nPress Enter to terminate...");

            var grain = GrainClient.GrainFactory.GetGrain<ICryptoGrain>(Guid.NewGuid());
            var task = grain.Hello( new Message("Muchachos"));
            task.Wait();
            Console.Write(task.Result.Data);
            Console.ReadLine();
            hostDomain.DoCallBack(ShutdownSilo);
        }

        static void InitSilo(string[] args)
        {
            hostWrapper = new OrleansHostWrapper(args);

            if (!hostWrapper.Run())
            {
                Console.Error.WriteLine("Failed to initialize Orleans silo");
            }
        }

        static void ShutdownSilo()
        {
            if (hostWrapper != null)
            {
                hostWrapper.Dispose();
                GC.SuppressFinalize(hostWrapper);
            }
        }

        private static OrleansHostWrapper hostWrapper;
    }
}
