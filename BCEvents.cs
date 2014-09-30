using System;
using KSP.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OATBeanCounter
{

    public class BCEvents
    {
        public static BCEvents instance = new BCEvents();
		public bool eventsAdded;

        public BCEvents()
        {
            eventsAdded = false;
        }

        public void addEvents()
        {
			GameEvents.onVesselCreate.Add(vesselCreateEvent);
			GameEvents.OnVesselRollout.Add(vesselRolloutEvent);
			GameEvents.OnFundsChanged.Add(fundsChangedEvent);

            eventsAdded = true;
		}

		public void fundsChangedEvent(double diff)
		{
			BeanCounter.LogFormatted_DebugOnly("Funds changed by {0:f2}. New funds: {1:f2}", diff, Funding.Instance.Funds);
		}
		
		public void vesselCreateEvent(Vessel vessel)
		{
			BeanCounter.LogFormatted_DebugOnly("vesselCreateEvent: {0} {1}", vessel.vesselName, vessel.state);
		}
		
		public void vesselRolloutEvent(ShipConstruct ship)
		{
			BeanCounter.LogFormatted_DebugOnly("vesselRolloutEvent: {0}", ship.shipName);

			float dryCost, fuelCost, totalCost;
			totalCost = ship.GetShipCosts (out dryCost, out fuelCost);
			BeanCounter.LogFormatted_DebugOnly("Total Cost: {0:f2}", totalCost);
			BeanCounter.LogFormatted_DebugOnly("Dry Cost: {0:f2}", dryCost);
			BeanCounter.LogFormatted_DebugOnly("Fuel Cost: {0:f2}", fuelCost);

			BCLaunchData launch = new BCLaunchData ();

			launch.vesselName = ship.shipName;
			launch.dryCost = dryCost;

			List<BCVesselResourceData> resources = new List<BCVesselResourceData>();
			foreach (Part part in ship.parts)
			{
				foreach (PartResource res in part.Resources)
				{
					if (res.info.unitCost == 0 || res.amount == 0)
					{
						// Don't need to keep track of free resources
						// Or maybe we should, in case the cost changes due to a mod/game update?
						continue;
					}
					
					BCVesselResourceData vr = resources.Find(r => r.resourceName == res.resourceName);
					if (vr == null)
					{
						resources.Add(new BCVesselResourceData(res.info, res.resourceName, res.amount, res.maxAmount));
					}
					else
					{
						vr.Add(res);
					}
				}
			}

			launch.resources = resources;

			OATBeanCounterData.data.launches.Add(launch);
		}
    }
}