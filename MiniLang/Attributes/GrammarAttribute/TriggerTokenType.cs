using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.Attributes.GrammarAttribute
{
    public class TriggerTokenType:Attribute
    {
        public TiggerType TriggerType { get;}
        public TriggerTokenType(TiggerType TriggerType)
        {
            this.TriggerType = TriggerType;
        }
    }
}
