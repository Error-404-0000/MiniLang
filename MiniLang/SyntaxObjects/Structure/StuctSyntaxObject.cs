using MiniLang.StructCreation;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniLang.SyntaxObjects.Structure
{
    public class StructSyntaxObject
    {
        public required StructFieldHandler StructHandler {  get; set; }
        public string StructName { get; set; }


    }
    
}
