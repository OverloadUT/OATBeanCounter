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
		[Persistent] public List<BCRecoveryData> recoveries = new List<BCRecoveryData>();
		[Persistent] public double funds = 0;

        public override void OnDecodeFromConfigNode()
		{
        }

        public override void OnEncodeToConfigNode()
        {
			// TODO this is probably a dumb place to do this? Should have a proper data update system, but YAGNI
			data_version = BeanCounter.VERSION;
        }
	}

	// TODO this might need to move to some sort of larger VesselEvent type data
	public class BCRecoveryData : ConfigNodeStorage
	{
		// TODO need resources. Might need to create a ResourceList class
		[Persistent] public Guid guid = Guid.NewGuid();
		[Persistent] public Guid transactionGuid;
		[Persistent] public float recoveryFactor = 0;
		[Persistent] public List<uint> partIDs = new List<uint>();
		[Persistent] public double time = HighLogic.fetch.currentGame.UniversalTime;
		
		public override void OnDecodeFromConfigNode()
		{
		}
		
		public override void OnEncodeToConfigNode()
		{
		}
	}

	public enum BCTransactionReasons {None};
	
	public class BCTransactionData : ConfigNodeStorage
	{
		[Persistent] public Guid guid = Guid.NewGuid();
		/// <summary>
		/// The Guid of another BCData object based on the TransactionReason
		/// For example, in a VesselRecovery transaction, this refers to a BCRecoveryData guid
		/// </summary>
		[Persistent] public Guid dataGuid;
		[Persistent] public double amount = 0;
		[Persistent] public double balance = 0;
		[Persistent] public double time = HighLogic.fetch.currentGame.UniversalTime;
		[Persistent] public BCTransactionReasons otherreason = BCTransactionReasons.None;
		[Persistent] public TransactionReasons reason = TransactionReasons.None;
		
		public BCTransactionData()
		{
		}
		
		public override void OnDecodeFromConfigNode()
		{
		}
		
		public override void OnEncodeToConfigNode()
		{
		}
	}

	public class BCLaunchData : ConfigNodeStorage
	{
		[Persistent] public Guid guid = Guid.NewGuid();
		[Persistent] public Guid transactionGuid;

		[Persistent] public string vesselName = "Unknown Craft";
		[Persistent] public uint missionID = 0;
		[Persistent] public float dryCost = 0;
		[Persistent] public float resourceCost = 0;
		[Persistent] public float totalCost = 0;
		[Persistent] public double launchTime = HighLogic.fetch.currentGame.UniversalTime;
		[Persistent] public List<BCVesselResourceData> resources = new List<BCVesselResourceData>();
		[Persistent] public List<BCVesselPartData> parts = new List<BCVesselPartData>();

		public override void OnDecodeFromConfigNode()
		{
		}
		
		public override void OnEncodeToConfigNode()
		{
		}
	}

	public enum BCVesselPartStatus {Unknown, Active, Debris, Recovered, Destroyed};

	public class BCVesselPartData : ConfigNodeStorage
	{
		[Persistent] public string partName = "UnknownPart";
		/// <summary>
		/// The base DRY cost of the part. Does not include costs added by modules.
		/// </summary>
		[Persistent] public float baseCost = 0;
		/// <summary>
		/// The sum of all of the extra costs added by modules
		/// </summary>
		[Persistent] public float moduleCosts = 0;
		/// <summary>
		/// The status of this part:
		/// Unknown: Shouldn't ever be used; means something went wrong
		/// Active: Part of a valid vessel
		/// Debris: Detached from a vessel and became debris
		/// Recovered: Recovered for a partial or full refund
		/// Destroyed: Part was destroyed
		/// </summary>
		[Persistent] public BCVesselPartStatus status = BCVesselPartStatus.Unknown;
		/// <summary>
		/// The unique ID of this part. Comes from Part.flightID
		/// </summary>
		[Persistent] public uint uid = 0;

		/// <summary>
		/// Gets the total DRY cost of the part, including any module-added costs
		/// </summary>
		/// <value>The total DRY cost of the part</value>
		public float cost
		{
			get
			{
				return baseCost + moduleCosts;
			}
		}
		
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