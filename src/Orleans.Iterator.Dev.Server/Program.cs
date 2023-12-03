﻿using Orleans.Configuration;
using Orleans.Iterator.AdoNet.Extensions;

var configuration = new ConfigurationBuilder()
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddEnvironmentVariables()
	.AddUserSecrets<Program>()
	.Build();

var storageType = configuration["StorageType"] ?? "";
var adoNetConnectionString = configuration["AdoNet:ConnectionString"] ?? "";
var adoNetInvariant = configuration["AdoNet:Invariant"] ?? "";

//
// NOTE: storageName is the 2nd parameter in the PersistentState grain attribute,
// for example:
//
// public ReverseGrain(
//     [PersistentState("Reverse","STORE_NAME")]
//     IPersistentState<ReverseState> state)
// {
//    ...
// }
//
var storageNames = new[]
{
	"STORE_NAME",
};

var builder = Host.CreateDefaultBuilder(args);

builder.UseOrleans((hostContext, siloBuilder) =>
{
	switch (storageType)
	{
		case "AdoNet":
			siloBuilder
				.UseAdoNetClustering(o =>
				{
					o.Invariant = adoNetInvariant;
					o.ConnectionString = adoNetConnectionString;
				});
			
			foreach (var storageName in storageNames)
			{
				siloBuilder
					.AddAdoNetGrainStorage(storageName, o =>
					{
						o.Invariant = adoNetInvariant;
						o.ConnectionString = adoNetConnectionString;
					});
			}
			
			siloBuilder
				.UseAdoNetGrainIterator(o =>
				{
					o.Invariant = adoNetInvariant;
					o.ConnectionString = adoNetConnectionString;
					o.IgnoreNullState = true;
				});
			break;
		
	}
	
	siloBuilder
		.Configure<ClusterOptions>(o =>
		{
			o.ClusterId = "iteartor";
			o.ServiceId = "tester";
		});
});

var host = builder.Build();
await host.RunAsync();


