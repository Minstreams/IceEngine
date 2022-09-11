using System.Collections.Generic;
using UnityEngine;
using IceEngine.DebugUI;
using System.Text.RegularExpressions;

namespace IceEngine.DebugUI.Internal
{
    [AddComponentMenu("|UI/Logger")]
    public class Logger : LoggerBase
    {
        public string regexExpr;
        protected override string RegexExpr => regexExpr;
    }
}
