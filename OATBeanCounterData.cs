using System;
using KSP.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OATBeanCounter
{

    public class OATBeanCounterData : ScenarioModule
    {
        public static BCDataStorage data = new BCDataStorage();

		public override void OnSave(ConfigNode node)
		{
            node.AddNode(data.AsConfigNode);

			// TODO I'd like to save a copy outside the persistence file as well
		}
		
		public override void OnLoad(ConfigNode node)
		{
			BeanCounter.LogFormatted_DebugOnly("Loading scenario node from persistence file");
			
			base.OnLoad(node);

            ConfigNode CN = node.GetNode(data.GetType().Name);
            if (CN != null)
                ConfigNode.LoadObjectFromConfig(data, CN);
		}
    }

    public class BCDataStorage : ConfigNodeStorage
    {
        [Persistent] public int persistInt = 1;
        [Persistent] public string data_version = "none";
		[Persistent] public List<BCLaunchData> launches = new List<BCLaunchData>();
		[Persistent] public List<BCTransactionData> transactions = new List<BCTransactionData>();
		[Persistent] public double funds = 0;

        public override void OnDecodeFromConfigNode()
        {
        }

        public override void OnEncodeToConfigNode()
        {
			// TODO this is probably a dump place to do this? Should have a proper data update system, but YAGNI
			data_version = BeanCounter.VERSION;
        }
	}
	
	public class BCTransactionData : ConfigNodeStorage
	{
		// TODO need to track WHAT the transaction was, probably with an enum? Struct?
		[Persistent] public double amount = 0;
		[Persistent] public double balance = 0;
		[Persistent] public double time = HighLogic.fetch.currentGame.UniversalTime;
		
		public override void OnDecodeFromConfigNode()
		{
		}
		
		public override void OnEncodeToConfigNode()
		{
		}
	}

	public class BCLaunchData : ConfigNodeStorage
	{
		[Persistent] public string vesselName = "Unknown Craft";
		[Persistent] public Guid id = new Guid();
		[Persistent] public int missionID = -1;
		[Persistent] public float dryCost = 0;
		[Persistent] public float resourceCost = 0;
		[Persistent] public float totalCost = 0;
		[Persistent] public List<BCVesselResourceData> resources = new List<BCVesselResourceData>();
		
		public override void OnDecodeFromConfigNode()
		{
		}
		
		public override void OnEncodeToConfigNode()
		{
		}
	}

	public class BCVesselResourceData : ConfigNodeStorage
	{
		// TODO figure out how to store this
		public PartResourceDefinition info;
		[Persistent] public double maxAmount = 0;
		[Persistent] public double amount = 0;
		[Persistent] public string resourceName = "UnknownResource";
		
		public override void OnDecodeFromConfigNode()
		{
		}
		
		public override void OnEncodeToConfigNode()
		{
		}

		// THIS IS NECESSARY FOR ENCODING TO A CONFIGNODE. MUST ALWAYS HAVE A ZERO-PARAMETER CONSTRUCTOR!
		public BCVesselResourceData() {} 

		public BCVesselResourceData(PartResourceDefinition partinfo, string name, double amt, double max)
		{
			amount = amt;
			maxAmount = max;
			resourceName = name;
			info = partinfo;
		}
		
		public void Add(PartResource pr) {
			amount += pr.amount;
			maxAmount += pr.maxAmount;
		}
		
		public double mass
		{
			get
			{
				return amount * info.density;
			}
		}
		
		public double cost
		{
			get
			{
				return amount * info.unitCost;
			}
		}
	}
}