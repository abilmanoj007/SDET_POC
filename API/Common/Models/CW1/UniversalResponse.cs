namespace SDET_POC.API.Common.Models.CW1;
/// <remarks/>
[System.CodeDom.Compiler.GeneratedCode("xsd", "4.8.3928.0")]
[Serializable()]
[System.Diagnostics.DebuggerStepThrough()]
[System.ComponentModel.DesignerCategory("code")]
[System.Xml.Serialization.XmlType(Namespace = "http://www.cargowise.com/Schemas/Universal/2011/11")]
[System.Xml.Serialization.XmlRoot("UniversalResponse", Namespace = "http://www.cargowise.com/Schemas/Universal/2011/11", IsNullable = false)]
public class UniversalResponseData
{

    private string statusField;

    private System.Xml.XmlElement dataField;

    private string processingLogField;

    private string versionField;

    /// <remarks/>
    public string Status
    {
        get
        {
            return statusField;
        }
        set
        {
            statusField = value;
        }
    }

    /// <remarks/>
    public System.Xml.XmlElement Data
    {
        get
        {
            return dataField;
        }
        set
        {
            dataField = value;
        }
    }

    /// <remarks/>
    public string ProcessingLog
    {
        get
        {
            return processingLogField;
        }
        set
        {
            processingLogField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttribute(DataType = "token")]
    public string version
    {
        get
        {
            return versionField;
        }
        set
        {
            versionField = value;
        }
    }
}


