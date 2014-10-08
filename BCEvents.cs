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
			GameEvents.onVesselRecoveryProcessing.Add(vesselRecoveryProcessingEvent);

            eventsAdded = true;
		}
		
		public void vesselRecoveryProcessingEvent(ProtoVessel pvessel, MissionRecoveryDialog dialog, float recoveryFactor)
		{
			BeanCounter.LogFormatted_DebugOnly("---------- vesselRecoveryProcessingEvent ------------");
			BeanCounter.LogFormatted_DebugOnly("recoveryFactor: {0:f3}", recoveryFactor);
			BeanCounter.LogFormatted_DebugOnly("Vessel root missionID: {0}", BCUtils.GetVesselMissionID(pvessel));

			// Get a list of every missionID from the recovered parts
			List<uint> recovered_mission_ids = 
				(from ppart in pvessel.protoPartSnapshots
				 select ppart.missionID).ToList();

			// Get a list of every unique part ID so we can match them up
			List<uint> recovered_part_ids = 
				(from ppart in pvessel.protoPartSnapshots
				 select ppart.flightID).ToList();

			// Now lets get all of the launches that contain recovered parts
			List<BCLaunchData> recovered_launches =
				(from launch in OATBeanCounterData.data.launches
				 where recovered_mission_ids.Contains(launch.missionID)
				 select launch).ToList();

			// And finally we get the full list of every recovered part so we can flag them all as recovered
			var recoveredparts =
				from launch in recovered_launches
				from part in launch.parts
				where recovered_part_ids.Contains(part.uid)
				select part;

			foreach(BCVesselPartData partdata in recoveredparts)
			{
				BeanCounter.LogFormatted_DebugOnly("Flagging part as recovered: {0} - {1}", partdata.partName, partdata.uid);
				partdata.status = BCVesselPartStatus.Recovered;
			}

			BCRecoveryData recovery = new BCRecoveryData();
			recovery.partIDs = recovered_part_ids;
			recovery.recoveryFactor = recoveryFactor;
			OATBeanCounterData.data.recoveries.Add(recovery);

			BeanCounter.LogFormatted_DebugOnly("--------- /vesselRecoveryProcessingEvent ------------");
		}

		public void fundsChangedEvent(double newfunds, TransactionReasons reason)
		{
			double diff = newfunds - OATBeanCounterData.data.funds;


			BeanCounter.LogFormatted_DebugOnly("Funds changed. New funds: {0:f2}", newfunds);
			BeanCounter.LogFormatted_DebugOnly("Change amount: {0:f2}", diff);

			BCTransactionData transaction = new BCTransactionData();
			transaction.amount = diff;
			transaction.balance = newfunds;
			transaction.time = HighLogic.fetch.currentGame.UniversalTime;
			transaction.reason = reason;
			OATBeanCounterData.data.transactions.Add(transaction);
			OATBeanCounterData.data.funds = newfunds;

			switch (transaction.reason)
			{
			case TransactionReasons.VesselRecovery:
				BCRecoveryData recovery =
					(from rec in OATBeanCounterData.data.recoveries
					 where rec.time == HighLogic.fetch.currentGame.UniversalTime
					 select rec).Single();
				recovery.transactionGuid = transaction.guid;
				transaction.dataGuid = recovery.guid;
				break;
			case TransactionReasons.VesselRollout:
				BCLaunchData launch =
					(from l in OATBeanCounterData.data.launches
					 where l.launchTime == HighLogic.fetch.currentGame.UniversalTime
					 select l).Single();
				launch.transactionGuid = transaction.guid;
				transaction.dataGuid = launch.guid;
				break;
			}
		}

		/// <summary>
		/// Fires every time a vessel object is created.
		/// Fires at the beginning of every scene for every vessel in the universe
		/// </summary>
		/// <param name="vessel">The vessel that was created</param>
		public void vesselCreateEvent(Vessel vessel)
		{
//			if(vessel.vesselType == VesselType.Unknown || vessel.vesselType == VesselType.SpaceObject)
//			{
//				// Ignore asteroids
//			} else {
//				BeanCounter.LogFormatted_DebugOnly("------------- vesselCreateEvent -------------");
//				BeanCounter.LogFormatted_DebugOnly("vesselName: {0}", vessel.vesselName);
//				BeanCounter.LogFormatted_DebugOnly("vesselType: {0}", vessel.vesselType);
//				BeanCounter.LogFormatted_DebugOnly("GetName(): {0}", vessel.GetName());
//				BeanCounter.LogFormatted_DebugOnly("name: {0}", vessel.name);
//				BeanCounter.LogFormatted_DebugOnly("RevealName(): {0}", vessel.RevealName());
//				BeanCounter.LogFormatted_DebugOnly("Vessel state: {0}", vessel.state);
//				BeanCounter.LogFormatted_DebugOnly("Vessel situation: {0}", vessel.situation);
//				BeanCounter.LogFormatted_DebugOnly("Part Count: {0}", vessel.Parts.Count);
//				BeanCounter.LogFormatted_DebugOnly("id: {0}", vessel.id);
//				BeanCounter.LogFormatted_DebugOnly("launchTime: {0}", vessel.launchTime);
//				BeanCounter.LogFormatted_DebugOnly("UniversalTime: {0}", HighLogic.fetch.currentGame.UniversalTime);
//				BeanCounter.LogFormatted_DebugOnly("New launch? {0}", (HighLogic.fetch.currentGame.UniversalTime == vessel.launchTime));
//				BeanCounter.LogFormatted_DebugOnly("Mission Time: {0}", vessel.missionTime);
//				BeanCounter.LogFormatted_DebugOnly("Vessel root missionID: {0}", BCUtils.GetVesselMissionID(vessel));
//				BeanCounter.LogFormatted_DebugOnly("----------- /vesselCreateEvent -------------");
//			}
		}
		
		public void vesselChangeEvent(Vessel vessel)
		{
//			if(vessel.vesselType == VesselType.Unknown || vessel.vesselType == VesselType.SpaceObject)
//			{
//				// Ignore asteroids
//			} else {
//				BeanCounter.LogFormatted_DebugOnly("------------- vesselChangeEvent -------------");
//				BeanCounter.LogFormatted_DebugOnly("name: {0}", vessel.vesselName);
//				BeanCounter.LogFormatted_DebugOnly("id: {0}", vessel.id);
//				BeanCounter.LogFormatted_DebugOnly("Vessel situation: {0}", vessel.situation);
//				BeanCounter.LogFormatted_DebugOnly("Vessel root missionID: {0}", BCUtils.GetVesselMissionID(vessel));
//				
//				BeanCounter.LogFormatted_DebugOnly("FlightGlobals.Vessels.Count: {0}", FlightGlobals.Vessels.Count);
//
//				BeanCounter.LogFormatted_DebugOnly("------------ /vesselChangeEvent -------------");
//			}
		}
		
		public void vesselSituationChangeEvent(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> ev)
		{
//			Vessel vessel = ev.host;
//			
//			if(vessel.vesselType == VesselType.Unknown || vessel.vesselType == VesselType.SpaceObject)
//			{
//				// Ignore asteroids
//			} else {
//				BeanCounter.LogFormatted_DebugOnly("------------- vesselSituationChangeEvent -------------");
//				BeanCounter.LogFormatted_DebugOnly("name: {0}", vessel.vesselName);
//				BeanCounter.LogFormatted_DebugOnly("id: {0}", vessel.id);
//				BeanCounter.LogFormatted_DebugOnly("from: {0}", ev.from);
//				BeanCounter.LogFormatted_DebugOnly("to: {0}", ev.to);
//				BeanCounter.LogFormatted_DebugOnly("Vessel root missionID: {0}", BCUtils.GetVesselMissionID(vessel));
//				BeanCounter.LogFormatted_DebugOnly("------------ /vesselSituationChangeEvent -------------");
//			}
		}

		/// <summary>
		/// Fires when a new vessel is created at the launch pad.
		/// </summary>
		/// <param name="ship">The ship being rolled out.</param>
		public void vesselRolloutEvent(ShipConstruct ship)
		{
			BeanCounter.LogFormatted_DebugOnly("------------- vesselRolloutEvent -------------");

			float dryCost, fuelCost, totalCost;
			totalCost = ship.GetShipCosts (out dryCost, out fuelCost);


			BeanCounter.LogFormatted_DebugOnly("Rollout: {0}", ship.shipName);
			BeanCounter.LogFormatted_DebugOnly("Total Cost: {0:f2}", totalCost);
			BeanCounter.LogFormatted_DebugOnly("Dry Cost: {0:f2}", dryCost);
			BeanCounter.LogFormatted_DebugOnly("Fuel Cost: {0:f2}", fuelCost);
			BeanCounter.LogFormatted_DebugOnly("launchID: {0}", HighLogic.fetch.currentGame.launchID);


			Vessel vessel = FlightGlobals.ActiveVessel;
			BeanCounter.LogFormatted_DebugOnly("NEW VESSEL LAUNCH DETECTED: {0}", vessel.vesselName);
			
			BCLaunchData launch = new BCLaunchData ();
			launch.vesselName = vessel.vesselName;
			launch.missionID = BCUtils.GetVesselMissionID(vessel);
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
				part_data.status = BCVesselPartStatus.Active;
				part_data.uid = part.flightID;
				parts.Add(part_data);

				total_resource_cost += part_resource_cost_actual;
			}
			
			launch.resources = resources;
			launch.parts = parts;
			launch.resourceCost = total_resource_cost;
			OATBeanCounterData.data.launches.Add(launch);

			BeanCounter.LogFormatted_DebugOnly("------------ /vesselRolloutEvent -------------");
		}
    }
}