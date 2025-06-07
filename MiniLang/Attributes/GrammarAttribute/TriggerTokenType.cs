using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Attributes.GrammarAttribute
{
    public class TriggerTokenType:Attribute
    {
        public TriggerType TriggerType { get;}
        public TriggerTokenType(TriggerType TriggerType)
        {
            this.TriggerType = TriggerType;
        }
    }
}
