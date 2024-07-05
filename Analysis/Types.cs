using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Borzoi;

public interface VTypeMod 
{
    public bool Same(VTypeMod other);

    public static VFunc Fn(params VType[] args) => new VFunc() { Args = args.ToList() };
    public static VFunc Fn(IEnumerable<VType> args) => new VFunc() { Args = args.ToList() };
    public static VFunc CFn(params VType[] args) => new VFunc() { CFunc = true, Args = args.ToList() };
    public static VFunc CFn(IEnumerable<VType> args) => new VFunc() { CFunc = true, Args = args.ToList() };
    public static VArray Arr() => new VArray();
    public static VArray Arr(int size) => new VArray(size);
    public static VPointer Pointer() => new VPointer();
}

public class VFunc : VTypeMod 
{
    public bool Same(VTypeMod other) => 
        other is VFunc func &&
        func.Args.Count == this.Args.Count &&
        this.Args.Select((a, i) => a == func.Args[i]).All(b => b);

    public List<VType> Args = new();
    public bool CFunc = false;

    public override string ToString() { return "(" + string.Join(", ", Args) + ")"; }
}

public class VArray : VTypeMod
{
    public bool Same(VTypeMod other) =>
        other is VArray arr &&
        (arr.Size == this.Size || !arr.Fixed || !this.Fixed);

    public bool Fixed;
    public int Size;

    public VArray() { Fixed = false; Size = -1; }
    public VArray(int size) { Fixed = true; Size = size; }
}

public class VPointer : VTypeMod
{
    public bool Same(VTypeMod other) => other is VPointer;
    public VPointer() {}
}

// TODO: Readonly members (can't set, or create pointers to)
public class TypeInfo
{
    public int ByteSize { get; set; }
    public int Alignment { get; set; }

    public List<(string Name, VType Type, int Offset)> Members = new();

    public TypeInfo(int bs, int al = -1) { ByteSize = bs; Alignment = al < 0 ? ByteSize : al; }

    public override string ToString() { return $"{ByteSize} | {Alignment}\n{string.Join("\n", Members.Select(member => $"{member.Name} :: {member.Type} // {member.Offset}")).Indent()}"; }

    public static readonly TypeInfo Pointer = new(Settings.Bytes);
    public static readonly TypeInfo Array = new(Settings.Bytes * 2, 8) { Members = { 
        ( "ptr", VType.Void.Modify(VTypeMod.Pointer()), 0 ), 
        ( "len", VType.Int, 8 ), 
    }};
}

// FUCK Equals() and GetHashCode(), all my homies FUCKING HATE Equals() and GetHashCode()
#pragma warning disable 0661, 0659
public class VType
{
    public static VType Int => new("int");
    public static VType I32 => new("i32");
    public static VType Byte => new("byte");

    public static VType Double => new("double");
    public static VType Float => new("float");

    public static VType Bool => new("bool");

    public static VType Void => new("void");



    public static VType VARARGS => new("$$$VARARGS$$$");

    public bool Valid;
    public string Name;
    public List<VTypeMod> Mods;

    public TypeInfo? GetInfo(Dictionary<string, TypeInfo> source)
    {
        if(this.Is<VPointer>()) return TypeInfo.Pointer;
        if(this.Is<VArray>()) return TypeInfo.Array;
        if(source is null) return null;
        if(!source.ContainsKey(this.Name)) return null;
        return source[this.Name];
    }

    public void RemoveLastMod() => Mods.RemoveAt(Mods.Count - 1);
    public VType Modify(VTypeMod mod) { Mods.Add(mod); return this; }

    public VType Copy() => Valid ? new (Name, new List<VTypeMod>(Mods)) : new();
    public static VType Invalid => new();
    public VType() { Valid = false; Name = ""; Mods = new(); }
    public VType(string name) : this(name, new List<VTypeMod>()) {}
    public VType(string name, params VTypeMod[] mods) : this(name, mods.ToList()) {}
    public VType(string name, List<VTypeMod> mods)
    {
        Valid = true;
        Name = name;
        Mods = mods;
    }

    public static bool operator ==(VType l, VType r) => l.Name == r.Name && l.Mods.Count == r.Mods.Count && l.Mods.Select((m, i) => m.Same(r.Mods[i])).All(b => b);
    public static bool operator !=(VType l, VType r) => !(l == r);
    public override bool Equals(object? obj) { return obj is not null && obj is VType type && type == this; }

    public override string ToString() { return (Name == "" ? "void" : Name) + string.Join("", Mods.Select(mod => mod switch { 
                VFunc fn => $"({string.Join(", ", fn.Args)})", 
                VArray arr => $"[{(arr.Fixed ? arr.Size.ToString() : "")}]",
                VPointer ptr => $"@",
                _ => throw new System.Exception("forgor") })); }

    public bool Is<T>() where T : VTypeMod => Mods.Count > 0 && Mods.Last() is T;
    public bool Is<T>(out T? mod) where T : VTypeMod 
    { 
        var m = Mods.LastOrDefault();
        if(m is null || m is T) mod = (T?)m;
        else mod = default;
        return Mods.Count > 0 && Mods.Last() is T; 
    }
}
