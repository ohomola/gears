﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

namespace Gears.Interpreter.Data.Serialization.JUnit
{ // 
// This source code was auto-generated by xsd, Version=4.6.1055.0.
// 


    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]
    public partial class failure {
    
        private string typeField;
    
        private string messageField;
    
        private string[] textField;
    
        /// <remarks/>
        [XmlAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string message {
            get {
                return this.messageField;
            }
            set {
                this.messageField = value;
            }
        }
    
        /// <remarks/>
        [XmlText()]
        public string[] Text {
            get {
                return this.textField;
            }
            set {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]
    public partial class error {
    
        private string typeField;
    
        private string messageField;
    
        private string[] textField;
    
        /// <remarks/>
        [XmlAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string message {
            get {
                return this.messageField;
            }
            set {
                this.messageField = value;
            }
        }
    
        /// <remarks/>
        [XmlText()]
        public string[] Text {
            get {
                return this.textField;
            }
            set {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]
    public partial class properties {
    
        private property[] propertyField;
    
        /// <remarks/>
        [XmlElement("property")]
        public property[] property {
            get {
                return this.propertyField;
            }
            set {
                this.propertyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]
    public partial class property {
    
        private string nameField;
    
        private string valueField;
    
        /// <remarks/>
        [XmlAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]
    public partial class skipped {
    
        private string typeField;
    
        private string messageField;
    
        private string[] textField;
    
        /// <remarks/>
        [XmlAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string message {
            get {
                return this.messageField;
            }
            set {
                this.messageField = value;
            }
        }
    
        /// <remarks/>
        [XmlText()]
        public string[] Text {
            get {
                return this.textField;
            }
            set {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]
    public partial class testcase {
    
        private skipped skippedField;
    
        private error[] errorField;
    
        private failure[] failureField;
    
        private string[] systemoutField;
    
        private string[] systemerrField;
    
        private string nameField;
    
        private string assertionsField;
    
        private string timeField;
    
        private string classnameField;
    
        private string statusField;
    
        /// <remarks/>
        public skipped skipped {
            get {
                return this.skippedField;
            }
            set {
                this.skippedField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("error")]
        public error[] error {
            get {
                return this.errorField;
            }
            set {
                this.errorField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("failure")]
        public failure[] failure {
            get {
                return this.failureField;
            }
            set {
                this.failureField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("system-out")]
        public string[] systemout {
            get {
                return this.systemoutField;
            }
            set {
                this.systemoutField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("system-err")]
        public string[] systemerr {
            get {
                return this.systemerrField;
            }
            set {
                this.systemerrField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string assertions {
            get {
                return this.assertionsField;
            }
            set {
                this.assertionsField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string time {
            get {
                return this.timeField;
            }
            set {
                this.timeField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string classname {
            get {
                return this.classnameField;
            }
            set {
                this.classnameField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string status {
            get {
                return this.statusField;
            }
            set {
                this.statusField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]
    public partial class testsuite {
    
        private property[] propertiesField;
    
        private testcase[] testcaseField;
    
        private string systemoutField;
    
        private string systemerrField;
    
        private string nameField;
    
        private string testsField;
    
        private string failuresField;
    
        private string errorsField;
    
        private string timeField;
    
        private string disabledField;
    
        private string skippedField;
    
        private string timestampField;
    
        private string hostnameField;
    
        private string idField;
    
        private string packageField;
    
        /// <remarks/>
        [XmlArrayItem("property", IsNullable=false)]
        public property[] properties {
            get {
                return this.propertiesField;
            }
            set {
                this.propertiesField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("testcase")]
        public testcase[] testcase {
            get {
                return this.testcaseField;
            }
            set {
                this.testcaseField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("system-out")]
        public string systemout {
            get {
                return this.systemoutField;
            }
            set {
                this.systemoutField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("system-err")]
        public string systemerr {
            get {
                return this.systemerrField;
            }
            set {
                this.systemerrField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string tests {
            get {
                return this.testsField;
            }
            set {
                this.testsField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string failures {
            get {
                return this.failuresField;
            }
            set {
                this.failuresField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string errors {
            get {
                return this.errorsField;
            }
            set {
                this.errorsField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string time {
            get {
                return this.timeField;
            }
            set {
                this.timeField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string disabled {
            get {
                return this.disabledField;
            }
            set {
                this.disabledField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string skipped {
            get {
                return this.skippedField;
            }
            set {
                this.skippedField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string timestamp {
            get {
                return this.timestampField;
            }
            set {
                this.timestampField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string hostname {
            get {
                return this.hostnameField;
            }
            set {
                this.hostnameField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string package {
            get {
                return this.packageField;
            }
            set {
                this.packageField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]
    public partial class testsuites {
    
        private testsuite[] testsuiteField;
    
        private string nameField;
    
        private string timeField;
    
        private string testsField;
    
        private string failuresField;
    
        private string disabledField;
    
        private string errorsField;
    
        /// <remarks/>
        [XmlElement("testsuite")]
        public testsuite[] testsuite {
            get {
                return this.testsuiteField;
            }
            set {
                this.testsuiteField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string time {
            get {
                return this.timeField;
            }
            set {
                this.timeField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string tests {
            get {
                return this.testsField;
            }
            set {
                this.testsField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string failures {
            get {
                return this.failuresField;
            }
            set {
                this.failuresField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string disabled {
            get {
                return this.disabledField;
            }
            set {
                this.disabledField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string errors {
            get {
                return this.errorsField;
            }
            set {
                this.errorsField = value;
            }
        }
    }
}