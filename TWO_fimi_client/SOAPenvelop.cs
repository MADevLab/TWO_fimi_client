﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// Этот исходный код был создан с помощью xsd, версия=4.8.3928.0.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.w3.org/2003/05/soap-envelope")]
[System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.w3.org/2003/05/soap-envelope", IsNullable=false)]
public partial class Envelope {
    
    private string bodyField;
    
    /// <remarks/>
    public string Body {
        get {
            return this.bodyField;
        }
        set {
            this.bodyField = value;
        }
    }
}
