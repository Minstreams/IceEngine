using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using IceEngine.Framework;

namespace IceEditor.Framework
{
    namespace Internal
    {
        public abstract class IceSystemDrawer
        {
            static List<IceSystemDrawer> _drawers = null;
            internal static List<IceSystemDrawer> Drawers
            {
                get
                {
                    if (_drawers == null)
                    {
                        _drawers = new();
                        var drawers = TypeCache.GetTypesDerivedFrom<IceSystemDrawer>();
                        foreach (var dt in drawers)
                        {
                            if (dt.IsAbstract) continue;
                            var drawer = (IceSystemDrawer)Activator.CreateInstance(dt);
                            _drawers.Add(drawer);
                        }
                    }
                    return _drawers;
                }
            }

            public abstract string SystemName { get; }
            public abstract void OnToolBoxGUI();
        }
    }
    public abstract class IceSystemDrawer<TSystem, TSetting> : Internal.IceSystemDrawer where TSetting : IceSetting<TSetting> where TSystem : IceSystem<TSetting>
    {
        public static TSetting Setting => IceSetting<TSetting>.Setting;
        public override string SystemName => typeof(TSystem).Name;
    }
}
