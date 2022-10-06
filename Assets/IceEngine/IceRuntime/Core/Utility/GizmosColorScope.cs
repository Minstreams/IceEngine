using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace IceEngine
{
    public class GizmosColorScope : IDisposable
    {
        Color originColor;
        public GizmosColorScope(Color color)
        {
            originColor = Gizmos.color;
            Gizmos.color = color;
        }
        void IDisposable.Dispose()
        {
            Gizmos.color = originColor;
        }
    }
}
