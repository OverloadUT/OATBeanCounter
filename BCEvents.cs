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

        protected double currencyModTime;
        protected List<CurrencyModifierQuery> lastCurrencyMods = new List<CurrencyModifierQuery>();

        public BCEvents()
        {
            eventsAdded = false;
        }

        public void addEvents()
		{
			GameEvents.onVesselChange.Add(vesselChangeEvent);
			GameEvents.onVesselSituationChange.Add(vesselSituationChangeEvent);
			GameEvents.OnVesselRollout.Add(vesselRolloutEvent);
			GameEvents.OnFundsChanged.Add(fundsChangedEvent);
			GameEvents.onVesselRecoveryProcessing.Add(vesselRecoveryProcessingEvent);
            GameEvents.onPartDie.Add(partDieEvent);
            GameEvents.Modifiers.OnCurrencyModified.Add(currencyModifiedEvent);
			
			BeanCounter.LogFormatted_DebugOnly("OATBeanCounter Events Hooked");

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

            BCRecoveryData recovery = new BCRecoveryData(true);
            OATBeanCounterData.data.recoveries.Add(recovery);

			recovery.partIDs = recovered_part_ids;
			recovery.recoveryFactor = recoveryFactor;

            // Try to match this to the transaction
            BCTransactionData transaction =
                (from trans in OATBeanCounterData.data.transactions
                 where trans.time == HighLogic.fetch.currentGame.UniversalTime
                 && trans.reason == TransactionReasons.VesselRecovery
                 select trans).SingleOrDefault();
            if (transaction != null)
            {
                BeanCounter.LogFormatted_DebugOnly("Found matching transaction for this recovery: {0}", transaction.id);
                recovery.transactionID = transaction.id;
                transaction.dataID = recovery.id;
            }

			BeanCounter.LogFormatted_DebugOnly("--------- /vesselRecoveryProcessingEvent ------------");
		}

        public void partDieEvent(Part part)
        {
            BeanCounter.LogFormatted_DebugOnly("---------- partDieEvent ------------");

            // Get the launch that this part was from
            BCLaunchData launch =
                (from launchq in OATBeanCounterData.data.launches
                 where launchq.missionID == part.missionID
                 select launchq).SingleOrDefault();

            if(launch == null)
            {
                BeanCounter.LogFormatted_DebugOnly("Could not find launch for missionID {0}", part.missionID);
                return;
            }

            // Get the VesselPartData for the part that died
            BCVesselPartData partdata =
                (from partq in launch.parts
                 where partq.uid == part.flightID
                 select partq).SingleOrDefault();

            if (partdata == null)
            {
                BeanCounter.LogFormatted_DebugOnly("Could not find part for flightID {0}", part.flightID);
                return;
            }

            BCPartDestructionData destruction = new BCPartDestructionData();
            destruction.time = HighLogic.CurrentGame.UniversalTime;

            partdata.status = BCVesselPartStatus.Destroyed;
            partdata.destruction = destruction;

            BeanCounter.LogFormatted_DebugOnly("--------- /partDieEvent ------------");
        }

        /// <summary>
        /// Fires every time a currency transaction is modified by a CurrencyModifierQuery object
        /// </summary>
        /// <param name="query">The object with all of the modification info</param>
        public void currencyModifiedEvent(CurrencyModifierQuery query)
        {
            BeanCounter.LogFormatted_DebugOnly("--------- currencyModifiedEvent ------------");

            BeanCounter.LogFormatted_DebugOnly("Currency modified! Reason: {0}, Funds {1:f2}, Science {2:f2}, Rep {3:f3}",
                query.reason.ToString(),
                query.GetEffectDelta(Currency.Funds),
                query.GetEffectDelta(Currency.Reputation),
                query.GetEffectDelta(Currency.Science)
                );

            //BeanCounter.LogFormatted_DebugOnly("Stack Trace: {0}", System.Environment.StackTrace);

            if (currencyModTime != HighLogic.CurrentGame.UniversalTime)
            {
                currencyModTime = HighLogic.CurrentGame.UniversalTime;
                lastCurrencyMods = new List<CurrencyModifierQuery>();
            }

            lastCurrencyMods.Add(query);

            BeanCounter.LogFormatted_DebugOnly("-------- /currencyModifiedEvent ------------");
        }

        /// <summary>
        /// Fires every time our Funds change. Logs the transaction, and does some magic to determine
        /// if the transaction was modified by an active Strategy, and logs that as well.
        /// </summary>
        /// <param name="newfunds">The new total Funds balance.</param>
        /// <param name="reason">The reason this change happened.</param>
		public void fundsChangedEvent(double newfunds, TransactionReasons reason)
        {
            // TODO: funds CAN change without this event firing. WHAT TO DO?????


            BeanCounter.LogFormatted_DebugOnly("--------- fundsChangedEvent ------------");
//            BeanCounter.LogFormatted_DebugOnly("Stack Trace? {0}", System.Environment.StackTrace);
    
			double diff = newfunds - OATBeanCounterData.data.funds;

			BeanCounter.LogFormatted_DebugOnly("Funds changed. New funds: {0:f2}", newfunds);
			BeanCounter.LogFormatted_DebugOnly("Change amount: {0:f2}", diff);

            BCTransactionData transaction = new BCTransactionData(true);
            OATBeanCounterData.data.transactions.Add(transaction);
            transaction.time = HighLogic.fetch.currentGame.UniversalTime;
            transaction.reason = reason;
            transaction.amount = diff;
            transaction.balance = newfunds;

            if (currencyModTime == HighLogic.CurrentGame.UniversalTime)
            {
                BeanCounter.LogFormatted_DebugOnly("  Checking cached queries. Count: {0}", lastCurrencyMods.Count());

                // We now take in to account EVERY modifierquery run on this frame.
                //CurrencyModifierQuery modquery = lastCurrencyMods.Find(
                //    q => q.GetInput(Currency.Funds) + q.GetEffectDelta(Currency.Funds) == diff);

                if (lastCurrencyMods.Count() > 0)
                {
                    // TODO: Record the reason for this transaction in addition to it being a strategy mod
                    float delta = lastCurrencyMods.Sum(q => q.GetEffectDelta(Currency.Funds));
                    double realcost = diff - delta;
                    BeanCounter.LogFormatted_DebugOnly("  Total modified by queries: Delta: {0}, Real Cost: {1}", delta, realcost);
                    transaction.amount = realcost;
                    transaction.balance = OATBeanCounterData.data.funds - delta;

                    BCTransactionData modtrans = new BCTransactionData(true);
                    OATBeanCounterData.data.transactions.Add(modtrans);
                    modtrans.time = HighLogic.fetch.currentGame.UniversalTime;
                    modtrans.reason = TransactionReasons.Strategies;
                    modtrans.balance = newfunds;
                    modtrans.amount = delta;
                }
            }
            OATBeanCounterData.data.funds = newfunds;

			// TODO this is awful
            // Also, it doesn't work anymore? This used to fire after the required events, but now it is before
			switch (transaction.reason)
			{
			case TransactionReasons.VesselRecovery:
				BCRecoveryData recovery =
					(from rec in OATBeanCounterData.data.recoveries
					 where rec.time == HighLogic.fetch.currentGame.UniversalTime
					 select rec).SingleOrDefault();
				if(recovery != null)
				{
					recovery.transactionID = transaction.id;
					transaction.dataID = recovery.id;
				}
				break;
			case TransactionReasons.VesselRollout:
				BCLaunchData launch =
					(from l in OATBeanCounterData.data.launches
					 where l.launchTime == HighLogic.fetch.currentGame.UniversalTime
					 select l).SingleOrDefault();
				if(launch != null)
				{
					launch.transactionID = transaction.id;
					transaction.dataID = launch.id;
				}
				break;
            }
            BeanCounter.LogFormatted_DebugOnly("-------- /fundsChangedEvent ------------");
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
			BeanCounter.LogFormatted_DebugOnly("launchID: {0}", HighLogic.fetch.currentGame.launchID);

			Vessel vessel = FlightGlobals.ActiveVessel;
			BeanCounter.LogFormatted_DebugOnly("NEW VESSEL LAUNCH DETECTED: {0}", vessel.vesselName);
			
			BCLaunchData launch = new BCLaunchData(true);
			launch.vesselName = vessel.vesselName;
			launch.missionID = BCUtils.GetVesselMissionID(vessel);
			launch.dryCost = dryCost;
			launch.totalCost = totalCost;
			launch.launchTime = vessel.launchTime;

			// TODO move this to utils?
			List<BCVesselResourceData> resources = new List<BCVesselResourceData>();
			List<BCVesselPartData> parts = new List<BCVesselPartData>();
			float total_resource_cost = 0;
			
            // Iterate over each part so we can log the parts, calculate the vessel resources, and
            // calculate the actual cost of everything
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
					
                    // Either create a new VesselResourceData, or add to the one we already have
                    // TODO perhaps this should be conbined in to a single static method on BCVesselResourceData?
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

            // Try to match this to the transaction
            BCTransactionData transaction =
                (from trans in OATBeanCounterData.data.transactions
                 where trans.time == HighLogic.CurrentGame.UniversalTime
                 && trans.reason == TransactionReasons.VesselRollout
                 select trans).SingleOrDefault();
            if (transaction != null)
            {
                BeanCounter.LogFormatted_DebugOnly("Found matching transaction for this rollout: {0}", transaction.id);
                launch.transactionID = transaction.id;
                transaction.dataID = launch.id;
            }

			BeanCounter.LogFormatted_DebugOnly("------------ /vesselRolloutEvent -------------");
		}
    }
}