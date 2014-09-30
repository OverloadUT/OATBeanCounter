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

			int i = 0;
			foreach(BCLaunchData launch in launches)
			{
				i++;
				BeanCounter.LogFormatted_DebugOnly("Launch {0}: {1}", i, launch.vesselName);

				int j = 0;
				foreach(BCVesselResourceData res in launch.resources)
				{
					BeanCounter.LogFormatted_DebugOnly("    Resource {0}: {1}", j, res.resourceName);
				}
			}
        }

        public override void OnEncodeToConfigNode()
        {
			BeanCounter.LogFormatted_DebugOnly("OnEncodeToConfigNode");
			BeanCounter.LogFormatted_DebugOnly("launches Count: {0}", launches.Count);
			data_version = BeanCounter.VERSION;
			
//			ConfigNode node_launches = new ConfigNode("launches");
//			node_launches = ConfigNode.CreateConfigFromObject(launches);
			
			int i = 0;
			foreach(BCLaunchData launch in launches)
			{
				i++;
				BeanCounter.LogFormatted_DebugOnly("Launch {0}: {1}", i, launch.vesselName);
				
				int j = 0;
				foreach(BCVesselResourceData res in launch.resources)
				{
					j++;
					BeanCounter.LogFormatted_DebugOnly("    Resource {0}: {1}", j, res.resourceName);
				}
			}
        }
    }

	public class BCLaunchData : ConfigNodeStorage
	{
		[Persistent] public string vesselName = "Unknown Craft";
		[Persistent] public int vesselID = 0;
		[Persistent] public float dryCost = 0;
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