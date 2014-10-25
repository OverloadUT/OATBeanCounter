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

        public static void NukeFromOrbit()
        {
            data = new BCDataStorage();
        }
    }

    public abstract class BCConfigNodeStorageID : ConfigNodeStorage
    {
        [Persistent] public uint id = 0;

        // HACK: we need to have a zero-parameter constructor or ConfigNode barfs on decode
        public BCConfigNodeStorageID() { }

        // HACK: we use this constructor when we are creating a new database entry so that it'll get a new ID
        // only when actually logging a new thing
        /// <summary>
        /// Construct a new storage object with a unique ID
        /// </summary>
        /// <param name="newID">Set this to true if this is a new entry that needs a new ID</param>
        public BCConfigNodeStorageID(bool newID)
        {
            if(newID)
            {
                id = ++OATBeanCounterData.data.index;
            }
        }
    }

    public class BCDataStorage : ConfigNodeStorage
    {
        [Persistent] public string data_version = "none";
		[Persistent] public List<BCLaunchData> launches = new List<BCLaunchData>();
		[Persistent] public List<BCTransactionData> transactions = new List<BCTransactionData>();
		[Persistent] public List<BCRecoveryData> recoveries = new List<BCRecoveryData>();
        // TODO: Figure out the best way to record initial funds
        // Probably want an initial transaction for starting funds
		[Persistent] public double funds = 0;

		[Persistent] public uint index = 0;

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
    public class BCRecoveryData : BCConfigNodeStorageID
	{
		// TODO need resources. Might need to create a ResourceList class
		[Persistent] public uint transactionID;
		[Persistent] public float recoveryFactor = 0;
		[Persistent] public List<uint> partIDs = new List<uint>();
		[Persistent] public double time = HighLogic.fetch.currentGame.UniversalTime;

        public BCRecoveryData() { }
        public BCRecoveryData(bool newID) : base(newID) { }

		public override void OnDecodeFromConfigNode()
        {
		}
		
		public override void OnEncodeToConfigNode()
        {
		}
	}

	public enum BCTransactionReasons {None, Parts, Fuel, LifeSupport, Resources};

    public class BCTransactionData : BCConfigNodeStorageID
	{
		/// <summary>
		/// The id of another BCData object based on the TransactionReason
		/// For example, in a VesselRecovery transaction, this refers to a BCRecoveryData id
		/// </summary>
		[Persistent] public uint dataID;
		[Persistent] public double amount = 0;
		[Persistent] public double balance = 0;
		[Persistent] public double time = HighLogic.fetch.currentGame.UniversalTime;
		[Persistent] public BCTransactionReasons otherreason = BCTransactionReasons.None;
		[Persistent] public TransactionReasons reason = TransactionReasons.None;

        public BCTransactionData() { }
        public BCTransactionData(bool newID) : base(newID) { }
		
		public override void OnDecodeFromConfigNode()
		{
		}
		
		public override void OnEncodeToConfigNode()
		{
		}
	}

    public class BCLaunchData : BCConfigNodeStorageID
	{
		[Persistent] public uint transactionID;
		[Persistent] public string vesselName = "Unknown Craft";
		[Persistent] public uint missionID = 0;
		[Persistent] public float dryCost = 0;
		[Persistent] public float resourceCost = 0;
		[Persistent] public float totalCost = 0;
		[Persistent] public double launchTime = HighLogic.fetch.currentGame.UniversalTime;
		[Persistent] public List<BCVesselResourceData> resources = new List<BCVesselResourceData>();
		[Persistent] public List<BCVesselPartData> parts = new List<BCVesselPartData>();

        public BCLaunchData() { }
        public BCLaunchData(bool newID) : base(newID) { }

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
        /// Contains extra information about this part's destruction, or null
        /// if this part is not destroyed
        /// </summary>
        [Persistent] public BCPartDestructionData destruction;

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

    public class BCPartDestructionData : ConfigNodeStorage
    {
        // TODO: record resources here?
        /// <summary>
        /// The time of destruction
        /// </summary>
        [Persistent] public double time;
        /// <summary>
        /// The name of the planetary body this was destroyed near
        /// </summary>
        [Persistent] public string body;

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