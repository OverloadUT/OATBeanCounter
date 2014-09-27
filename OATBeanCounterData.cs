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

        public override void OnDecodeFromConfigNode()
        {
            BeanCounter.LogFormatted_DebugOnly("OnDecodeFromConfigNode");
			BeanCounter.LogFormatted_DebugOnly("launches Count: {0}", launches.Count);
            BeanCounter.LogFormatted_DebugOnly(persistInt.ToString());
        }

        public override void OnEncodeToConfigNode()
        {
			BeanCounter.LogFormatted_DebugOnly("OnEncodeToConfigNode");
			BeanCounter.LogFormatted_DebugOnly("launches Count: {0}", launches.Count);
            data_version = BeanCounter.VERSION;
        }
    }

	public class BCLaunchData
	{
		[Persistent] public string vesselName;
		[Persistent] public int vesselID;
		[Persistent] public float dryCost;
		[Persistent] public List<BCVesselResourceData> resources;
	}

	public class BCVesselResourceData
	{
		[Persistent] public PartResourceDefinition info;
		[Persistent] public double maxAmount = 0;
		[Persistent] public double amount = 0;
		[Persistent] public string resourceName;
		
		public BCVesselResourceData(PartResourceDefinition partinfo, string name)
		{
			resourceName = name;
			info = partinfo;
		}
		
		public BCVesselResourceData(PartResourceDefinition partinfo, string name, double amt, double max) : this(partinfo, name)
		{
			amount = amt;
			maxAmount = max;
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