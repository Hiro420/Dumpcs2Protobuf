# Dumpcs2Protobuf
some shit i tried to make to extract the obfuscated protobuf messages from dump.cs file (needs to set manually the proto base class name)

# Target Games: HSR, GI (pre-3.5, not tested)
# YOU WILL NEED A FULL DUMP.CS FILE CONTAINING 100% FULL DUMP (INCLUDING ALL FULL FIELDS AND VALUES) ELSE IT WONT WORK


# USAGE

1- edit the "string searchString" in the code to match the proto base class name

2- build it via Visual Studio 2022

3- put the dump.cs in same folder as the executable

4- run the program

5- you will be left with an `output` folder, containing the protobuf definition. 

6- profit

# I WILL NOT GUIDE YOU ON HOW TO DEOBFUSCATE PROTOS, YOU CAN DO IT USING TOOLS OR MANUALLY IF YOU'RE CRAZY ENOUGH

### Example proto class:
```cs
// AssemblyIndex: 73 TypeIndexInAssembly: 4771
// TypeDefIndex: 21606
// Module: RPG.Network.Proto.dll
// Namespace: 
// FullName: IEFELCIJJDA
public sealed class IEFELCIJJDA : IMessage, IMessage<IEFELCIJJDA>, IEquatable<IEFELCIJJDA>, IDeepCloneable<IEFELCIJJDA>
{
	// Fields
	private static readonly MessageParser<IEFELCIJJDA> CHAOHFOCDEB;
	private UnknownFieldSet JALDALIDBOO;
	public const int CPBHCOPAGBD = 13;
	private uint AOJNJCGDHBB;
	public const int NEFBNAELMLD = 14;
	private static readonly FieldCodec<NFJLPFKFDLF> GKHOGKOMPFM;
	private readonly RepeatedField<NFJLPFKFDLF> OMOIAPKFPBC;

	// Properties
	[DebuggerNonUserCodeAttribute]
	public static MessageParser<IEFELCIJJDA> MGGKOEHEENL { get; }
	[DebuggerNonUserCodeAttribute]
	private MessageDescriptor OMGLPJBHFAO { get; }
	[DebuggerNonUserCodeAttribute]
	public uint EGPKLBOKFNF { get; set; }
	[DebuggerNonUserCodeAttribute]
	public RepeatedField<NFJLPFKFDLF> MDDLGJHLLAI { get; }

	// Constructors
	[DebuggerNonUserCodeAttribute]
	public void .ctor() // RVA: 0x03B6EED0
	[DebuggerNonUserCodeAttribute]
	public void .ctor(IEFELCIJJDA IDBLEINEBMO) // RVA: 0x03B6EE10
	private static void .cctor() // RVA: 0x03B6ECF0

	// Methods
	public static MessageParser<IEFELCIJJDA> LBDIKGFHEFF() // RVA: 0x03B6EA40
	private MessageDescriptor pb::Google.Protobuf.IMessage.get_Descriptor() // RVA: 0x00B90D10
	[DebuggerNonUserCodeAttribute]
	public IEFELCIJJDA Clone() // RVA: 0x03B6E750
	public uint IBNPDOJNDPO() // RVA: 0x00B00EA0
	public void HAMKADHBBJA(uint HONMPMJILHE) // RVA: 0x00B00EB0
	public RepeatedField<NFJLPFKFDLF> AMDIKNJGNNN() // RVA: 0x00AF4D50
	[DebuggerNonUserCodeAttribute]
	public override bool Equals(object IDBLEINEBMO) // RVA: 0x03B6E8D0
	[DebuggerNonUserCodeAttribute]
	public bool Equals(IEFELCIJJDA IDBLEINEBMO) // RVA: 0x03B6E840
	[DebuggerNonUserCodeAttribute]
	public override int GetHashCode() // RVA: 0x03B6E9B0
	[DebuggerNonUserCodeAttribute]
	public override string ToString() // RVA: 0x03B6EBE0
	[DebuggerNonUserCodeAttribute]
	public void WriteTo(CodedOutputStream PKLLLHGLILG) // RVA: 0x03B6EC30
	[DebuggerNonUserCodeAttribute]
	public int CalculateSize() // RVA: 0x03B6E690
	[DebuggerNonUserCodeAttribute]
	public void MergeFrom(IEFELCIJJDA IDBLEINEBMO) // RVA: 0x03B6EA90
	[DebuggerNonUserCodeAttribute]
	public void MergeFrom(CodedInputStream PFFEJAFDIGF) // RVA: 0x03B6EB10
}
```

### Example of enum class:
```cs
// AssemblyIndex: 73 TypeIndexInAssembly: 4781
// TypeDefIndex: 21616
// Module: RPG.Network.Proto.dll
// Namespace: 
// FullName: FGEMFOPIADP
public enum FGEMFOPIADP
{
	// Fields
	public int value__;
	[OriginalNameAttribute(Name = "CmdShopTypeNone", PreferredAlias = True)]
	public const FGEMFOPIADP IHJPPFIKELH = 0;
	[OriginalNameAttribute(Name = "CmdCityShopInfoScNotify", PreferredAlias = True)]
	public const FGEMFOPIADP PNKKDKOGMOM = 1533;
	[OriginalNameAttribute(Name = "CmdGetShopListScRsp", PreferredAlias = True)]
	public const FGEMFOPIADP MBBBOBNIMHF = 1571;
	[OriginalNameAttribute(Name = "CmdTakeCityShopRewardCsReq", PreferredAlias = True)]
	public const FGEMFOPIADP BNMFLHDJNIK = 1579;
	[OriginalNameAttribute(Name = "CmdGetShopListCsReq", PreferredAlias = True)]
	public const FGEMFOPIADP HJNDCEDJHHE = 1598;
	[OriginalNameAttribute(Name = "CmdTakeCityShopRewardScRsp", PreferredAlias = True)]
	public const FGEMFOPIADP DEHKAMAMCBO = 1577;
	[OriginalNameAttribute(Name = "CmdBuyGoodsCsReq", PreferredAlias = True)]
	public const FGEMFOPIADP OPINDNKEFNF = 1583;
	[OriginalNameAttribute(Name = "CmdBuyGoodsScRsp", PreferredAlias = True)]
	public const FGEMFOPIADP GOLLAMKKECD = 1542;
}
```