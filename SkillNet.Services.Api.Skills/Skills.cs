using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using SkillNet.Services.Models;
using Microsoft.ServiceFabric.Services.Client;

namespace SkillNet.Services.Api.Skills
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class Skills : StatefulService
    {

        private const string SkillDictionaryName = "SkillStore";

        public Skills(StatefulServiceContext context)
            : base(context)
        { }




        private async Task FillWithData(CancellationToken cancellationToken)
        {
            bool Done = false;
            IReliableDictionary<int, List<TechSkill>> dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<int, List<TechSkill>>>(SkillDictionaryName);


           
            //fill with sample data
            while (!Done)
            {
                try
                {
                    // Create a new Transaction object for this partition
                    using (ITransaction tx = base.StateManager.CreateTransaction())
                    {

                        IAsyncEnumerable<KeyValuePair<int, List<TechSkill>>> SkillEnum = (await dictionary.CreateEnumerableAsync(tx));

                        TechSkill tsk = new TechSkill
                        {
                            ResourceId = 1,
                            Name = "C#",
                            Level = Models.Domain.Level.Expert,
                            Notes = string.Empty
                        };

                        List<TechSkill> skills = new List<TechSkill>();
                        skills.Add(tsk);

                        await dictionary.AddAsync(tx, tsk.ResourceId, skills);

                        // CommitAsync sends Commit record to log & secondary replicas
                        // After quorum responds, all locks released
                        await tx.CommitAsync();
                    }
                    // If CommitAsync not called, Dispose sends Abort
                    // record to log & all locks released
                    Done = true;
                }
                catch (TimeoutException)
                {
                    Done = false;
                    await Task.Delay(100, cancellationToken);
                }

            }

        }



        protected override Task RunAsync(CancellationToken cancellationToken)
        {

            FillWithData(cancellationToken).RunSynchronously();

            return base.RunAsync(cancellationToken);

        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[]
            {
                new ServiceReplicaListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<StatefulServiceContext>(serviceContext)
                                            .AddSingleton<IReliableStateManager>(this.StateManager))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }
    }
}
