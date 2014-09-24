using System;
using KSP.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OATBeanCounter
{
    public class BCEditorWindow : MonoBehaviourWindow
    {
    	internal override void Awake()
    	{
            WindowRect = new Rect(0, 0, 200, 100);
            Visible =  true;
            DragEnabled = true;
            //WindowOptions[0] = GUILayout.ExpandHeight(true);
            WindowCaption="OAT Bean Counter v"+BeanCounter.VERSION;
    	}

        internal override void DrawWindow(int id)
        {
        	float cost = parts.Sum(p => p.partInfo.cost);
			GUILayout.Label(String.Format("Cost: {0:f2}", cost));
			GUILayout.Label(String.Format("Parts: {0}", parts.Count));
			
			GUILayout.Label("Resources:");
			foreach (VesselResource resource in resources)
			{
				GUILayout.BeginVertical("box");
				GUILayout.Label(resource.resourceName);
				GUILayout.Label(String.Format("{0:f2} tons", resource.mass));
				GUILayout.Label(String.Format("${0:f0}", resource.cost));
				GUILayout.EndVertical();
			}

			GUILayout.Label("Parts:");
			foreach (Part part in parts) {
				string partname = part.partInfo.title;
//				GUILayout.BeginVertical("box");
				GUILayout.Label(partname);
//				GUILayout.Label(String.Format("GetModuleCosts: {0:f2}", part.GetModuleCosts()));
//				GUILayout.Label(String.Format("entryCost: {0}", part.partInfo.entryCost));
//				GUILayout.Label(String.Format("cost: {0:f2}", part.partInfo.cost));
//				GUILayout.Label(String.Format("category: {0}", part.partInfo.category.ToString()));
//				GUILayout.EndVertical();
			}
        }

		internal override void Update()
		{
		}

		// TODO: currently only works in the editor. Blank list otherwise
        List<Part> parts
        {
            get
            {
            	if (HighLogic.LoadedSceneIsEditor) {
            		List<Part> parts = EditorLogic.fetch.ship.parts;
            		if (parts != null
            			&& parts.Count > 0) {
            			return parts;
            		} else {
            			return new List<Part>();
            		}
            	}

                return new List<Part>();
            }
        }

        List<VesselResource> resources
        {
        	get
        	{
        		List<VesselResource> resources = new List<VesselResource>();
        		foreach (Part part in parts)
        		{
					foreach (PartResource res in part.Resources)
					{
						if (res.info.unitCost == 0 || res.amount == 0)
						{
							continue;
						}

						VesselResource vr = resources.Find(r => r.resourceName == res.resourceName);
						if (vr == null)
						{
							resources.Add(new VesselResource(res.info, res.resourceName, res.amount, res.maxAmount));
						}
						else
						{
							vr.Add(res);
						}
					}
        		}

        		return resources;
        	}
        }
    }
}