# XmlORM
C#XML文件的ORM工具，读写终于可以对着对象操作一通了

调用示例在这里：


ADSXml xml = new ADSXml(@"E:\Users\G50-80\Desktop\Config.xml");
Device_Standard d= xml.Get<Device_Standard>();
d.FlowterName = "lsmt";
xml.Save<Device_Standard>(d);
xml.SaveSingle<Device_Standard>(it=>new Device_Standard {FlowterName="lsmt" });


[ADSXmlNode("Device_Standard")]
public class Device_Standard
{
    public string FlowterName { get; set; }
    public string FlowterDes { get; set; }
    public string FlowterSN { get; set; }
    public string FlowComNo { get; set; }
    public string TCPomNo { get; set; }
    public string DataBit { get; set; }
    public string BaudRate { get; set; }
    public string ParityBit { get; set; }
    public string StopBit { get; set; }
    public string AddressNo { get; set; }
    public string ByteOrder { get; set; }
    public string Price { get; set; }
    public string IsCheck { get; set; }
    public string CheckDays { get; set; }
    public string RegisterCode { get; set; }
}
