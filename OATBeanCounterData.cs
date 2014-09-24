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
        public bool isWorking = true;

        public override void OnSave(ConfigNode node)
        {
			node.AddValue ("isWorking", isWorking);
        }
    }
}