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
			GameEvents.onVesselChange.Add(vesselChangeEvent);
			GameEvents.onVesselSituationChange.Add(vesselSituationChangeEvent);
			GameEvents.OnVesselRollout.Add(vesselRolloutEvent);
			GameEvents.OnFundsChanged.Add(fundsChangedEvent);
			//GameEvents.onGUIRecoveryDialogSpawn.Add(recoveryDialogSpawnEvent);
            eventsAdded = true;
		}

		public void fundsChangedEvent(double newfunds)
		{
			double diff = newfunds - OATBeanCounterData.data.funds;

			BeanCounter.LogFormatted_DebugOnly("Funds changed. New funds: {0:f2}", newfunds);
			BeanCounter.LogFormatted_DebugOnly("Change amount: {0:f2}", diff);

			BCTransactionData transaction = new BCTransactionData();
			transaction.amount = diff;
			transaction.balance = newfunds;
			transaction.time = HighLogic.fetch.currentGame.UniversalTime;
			OATBeanCounterData.data.transactions.Add(transaction);
			OATBeanCounterData.data.funds = newfunds;
		}

		public void vesselCreateEvent(Vessel vessel)
		{
			if(vessel.vesselType == VesselType.Unknown || vessel.vesselType == VesselType.SpaceObject)
			{
				// Ignore asteroids
			} else {
				BeanCounter.LogFormatted_DebugOnly("------------- vesselCreateEvent -------------");
				BeanCounter.LogFormatted_DebugOnly("vesselName: {0}", vessel.vesselName);
				BeanCounter.LogFormatted_DebugOnly("vesselType: {0}", vessel.vesselType);
				BeanCounter.LogFormatted_DebugOnly("GetName(): {0}", vessel.GetName());
				BeanCounter.LogFormatted_DebugOnly("name: {0}", vessel.name);
				BeanCounter.LogFormatted_DebugOnly("RevealName(): {0}", vessel.RevealName());
				BeanCounter.LogFormatted_DebugOnly("Vessel state: {0}", vessel.state);
				BeanCounter.LogFormatted_DebugOnly("Vessel situation: {0}", vessel.situation);
				BeanCounter.LogFormatted_DebugOnly("Part Count: {0}", vessel.Parts.Count);
				BeanCounter.LogFormatted_DebugOnly("id: {0}", vessel.id);
				BeanCounter.LogFormatted_DebugOnly("launchTime: {0}", vessel.launchTime);
				BeanCounter.LogFormatted_DebugOnly("UniversalTime: {0}", HighLogic.fetch.currentGame.UniversalTime);
				BeanCounter.LogFormatted_DebugOnly("New launch? {0}", (HighLogic.fetch.currentGame.UniversalTime == vessel.launchTime));
				BeanCounter.LogFormatted_DebugOnly("Mission Time: {0}", vessel.missionTime);
				if(vessel.rootPart == null)
				{
					BeanCounter.LogFormatted_DebugOnly("Vessel has no rootPart");
				} else {
					BeanCounter.LogFormatted_DebugOnly("Vessel root missionID: {0}", vessel.rootPart.missionID);
				}
				BeanCounter.LogFormatted_DebugOnly("----------- /vesselCreateEvent -------------");
			}
		}
		
		public void vesselChangeEvent(Vessel vessel)
		{
			if(vessel.vesselType == VesselType.Unknown || vessel.vesselType == VesselType.SpaceObject)
			{
				// Ignore asteroids
			} else {
				BeanCounter.LogFormatted_DebugOnly("------------- vesselChangeEvent -------------");
				BeanCounter.LogFormatted_DebugOnly("name: {0}", vessel.vesselName);
				BeanCounter.LogFormatted_DebugOnly("id: {0}", vessel.id);
				BeanCounter.LogFormatted_DebugOnly("Vessel situation: {0}", vessel.situation);
				if(vessel.rootPart == null)
				{
					BeanCounter.LogFormatted_DebugOnly("Vessel has no rootPart");
				} else {
					BeanCounter.LogFormatted_DebugOnly("Vessel root missionID: {0}", vessel.rootPart.missionID);
				}
				
				BeanCounter.LogFormatted_DebugOnly("FlightGlobals.Vessels.Count: {0}", FlightGlobals.Vessels.Count);

				BeanCounter.LogFormatted_DebugOnly("------------ /vesselChangeEvent -------------");
			}
		}
		
		public void vesselSituationChangeEvent(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> ev)
		{
			Vessel vessel = ev.host;
			
			if(vessel.vesselType == VesselType.Unknown || vessel.vesselType == VesselType.SpaceObject)
			{
				// Ignore asteroids
			} else {
				BeanCounter.LogFormatted_DebugOnly("------------- vesselSituationChangeEvent -------------");
				BeanCounter.LogFormatted_DebugOnly("name: {0}", vessel.vesselName);
				BeanCounter.LogFormatted_DebugOnly("id: {0}", vessel.id);
				BeanCounter.LogFormatted_DebugOnly("from: {0}", ev.from);
				BeanCounter.LogFormatted_DebugOnly("to: {0}", ev.to);
				if(vessel.rootPart == null)
				{
					BeanCounter.LogFormatted_DebugOnly("Vessel has no rootPart");
				} else {
					BeanCounter.LogFormatted_DebugOnly("Vessel root missionID: {0}", vessel.rootPart.missionID);
				}
				BeanCounter.LogFormatted_DebugOnly("------------ /vesselSituationChangeEvent -------------");
			}
		}

		/// <summary>
		/// Fires when a new vessel is created at the launch pad.
		/// </summary>
		/// <param name="ship">The ship being rolled out.</param>
		public void vesselRolloutEvent(ShipConstruct ship)
		{
			float dryCost, fuelCost, totalCost;
			totalCost = ship.GetShipCosts (out dryCost, out fuelCost);


			BeanCounter.LogFormatted_DebugOnly("------------- vesselRolloutEvent -------------");
			BeanCounter.LogFormatted_DebugOnly("Rollout: {0}", ship.shipName);
			BeanCounter.LogFormatted_DebugOnly("Total Cost: {0:f2}", totalCost);
			BeanCounter.LogFormatted_DebugOnly("Dry Cost: {0:f2}", dryCost);
			BeanCounter.LogFormatted_DebugOnly("Fuel Cost: {0:f2}", fuelCost);
			BeanCounter.LogFormatted_DebugOnly("launchID: {0}", HighLogic.fetch.currentGame.launchID);
			BeanCounter.LogFormatted_DebugOnly("------------ /vesselRolloutEvent -------------");


			Vessel vessel = FlightGlobals.ActiveVessel;
			BeanCounter.LogFormatted_DebugOnly("NEW VESSEL LAUNCH DETECTED: {0}", vessel.vesselName);
			
			BCLaunchData launch = new BCLaunchData ();
			launch.vesselName = vessel.vesselName;
			launch.missionID = vessel.rootPart.missionID; // TODO null check this
			launch.dryCost = dryCost;
			launch.totalCost = totalCost;
			launch.launchTime = vessel.launchTime;

			// TODO move this to utils?
			List<BCVesselResourceData> resources = new List<BCVesselResourceData>();
			List<BCVesselPartData> parts = new List<BCVesselPartData>();
			float total_resource_cost = 0;
			
			foreach (Part part in vessel.parts)
			{
				BCVesselPartData part_data = new BCVesselPartData();
				part_data.partName = part.partInfo.name;
				
				float part_full_cost = part.partInfo.cost;
				float part_resource_cost_full = 0;
				float part_resource_cost_actual = 0;
				
				foreach (PartResource res in part.Resources)
				{
					if (res.info.unitCost == 0 || res.amount == 0)
					{
						// Don't need to keep track of free resources
						// Or maybe we should, in case the cost changes due to a mod/game update?
						continue;
					}
					
					part_resource_cost_full += (float)(res.info.unitCost * res.maxAmount);
					part_resource_cost_actual += (float)(res.info.unitCost * res.amount);
					
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
				
				float part_dry_cost = part_full_cost - part_resource_cost_full;
				part_data.baseCost = part_dry_cost;
				part_data.moduleCosts = part.GetModuleCosts();
				parts.Add(part_data);

				total_resource_cost += part_resource_cost_actual;
			}
			
			launch.resources = resources;
			launch.parts = parts;
			launch.resourceCost = total_resource_cost;

			OATBeanCounterData.data.launches.Add(launch);
		}
    }
}