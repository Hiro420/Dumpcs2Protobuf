using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace DumpFileParser;

public class ProtoMessage
{
    public string protoName = "";
    public List<string> importNameList = new();
    public List<ProtoFIeld> fieldList = new();
    public List<OneOf> oneOfList = new();    
}

public class ProtoFIeld
{
    public string fieldName = "";
    public string fieldType = "";
    public string? mapKey;
    public string? mapValue;
    public bool isRepeated;
    public uint val;
}

public class OneOf
{
    public string oneOfName = "";
    public List<ProtoFIeld> fieldList = new();
}

public class ProtoEnum
{
    public string enumName = "";
    public Dictionary<string, int> valDict = new();
}