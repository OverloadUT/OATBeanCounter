using System;
using KSP.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OATBeanCounter
{
    public class BCProfitLossWindow : MonoBehaviourWindow
    {


    	internal override void Awake()
    	{
            WindowRect = new Rect(0, 0, 200, 100);
            DragEnabled = true;
            //WindowOptions[0] = GUILayout.ExpandHeight(true);
            WindowCaption="BeanCounter Profit and Loss " + BeanCounter.VERSION;

            if(HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                Visible = true;
            }
    	}

        // TODO: EVERYTHING IN HERE IS HORRIBLE THIS IS JUST A PROOF OF CONCEPT
        internal override void DrawWindow(int id)
        {
            BCDataStorage data = OATBeanCounterData.data;

            // TODO: OH MY GOD SERIOUSLY DO NOT SHIP WITH THIS CODE

            // TransactionReasons.VesselRollout
            List<BCTransactionData> rollouts = 
                (from trans in OATBeanCounterData.data.transactions
                where trans.reason == TransactionReasons.VesselRollout
                select trans).ToList();

            double costs_rollout = rollouts.Sum(t => t.amount);
            GUILayout.Label(String.Format("Rollouts {0}", CurrencyFormat(costs_rollout)));

            // TransactionReasons.VesselRecovery
            List<BCTransactionData> recovery =
                (from trans in OATBeanCounterData.data.transactions
                 where trans.reason == TransactionReasons.VesselRecovery
                 select trans).ToList();

            double costs_recovery = recovery.Sum(t => t.amount);
            GUILayout.Label(String.Format("Recoveries {0}", CurrencyFormat(costs_recovery)));

            // TransactionReasons.Strategies
            List<BCTransactionData> strategies =
                (from trans in OATBeanCounterData.data.transactions
                 where trans.reason == TransactionReasons.Strategies
                 select trans).ToList();

            double costs_strategies = strategies.Sum(t => t.amount);
            GUILayout.Label(String.Format("Strategies {0}", CurrencyFormat(costs_strategies)));

            // TransactionReasons.StrategySetup
            List<BCTransactionData> strategysetup =
                (from trans in OATBeanCounterData.data.transactions
                 where trans.reason == TransactionReasons.StrategySetup
                 select trans).ToList();

            double costs_strategysetup = strategysetup.Sum(t => t.amount);
            GUILayout.Label(String.Format("Strategy Setups {0}", CurrencyFormat(costs_strategysetup)));
        }

		internal override void Update()
		{
		}

        public string CurrencyFormat(double value)
        {
            if(value < 0)
            {
                return String.Format("({0:f2})", Math.Abs(value));
            } else {
                return String.Format("{0:f2}", value);
            }
        }
    }
}