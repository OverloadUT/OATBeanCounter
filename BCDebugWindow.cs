using System;
using KSP.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OATBeanCounter
{
    public class BCDebugWindow : MonoBehaviourWindow
    {
    	internal override void Awake()
    	{
            WindowRect = new Rect(0, 0, 200, 100);
            Visible =  true;
            DragEnabled = true;
            //WindowOptions[0] = GUILayout.ExpandHeight(true);
            WindowCaption="OAT BC DEBUG v"+BeanCounter.VERSION;
    	}

        internal override void DrawWindow(int id)
        {
            if (GUILayout.Button("Reset database"))
            {
                OATBeanCounterData.NukeFromOrbit();
            }

            BCDataStorage data = OATBeanCounterData.data;

            GUILayout.Label(String.Format("Launches: {0}", data.launches.Count));
            GUILayout.Label(String.Format("Recoveries: {0}", data.recoveries.Count));
            GUILayout.Label(String.Format("Transactions: {0}", data.transactions.Count));
            GUILayout.Label(String.Format("Funds: {0}", data.funds));
        }

		internal override void Update()
		{
		}
    }
}