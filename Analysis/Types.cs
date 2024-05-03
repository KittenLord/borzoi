using System.Collections.Generic;
using System.Linq;

namespace EdComp;

public interface VTypeMod 
{
    public bool Same(VTypeMod other);

    public static VFunc Fn(params VType[] args) => new VFunc() { Args = args.ToList() };
    public static VFunc Fn(IEnumerable<VType> args) => new VFunc() { Args = args.ToList() };
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

// FUCK Equals() and GetHashCode(), all my homies FUCKING HATE Equals() and GetHashCode()
public class VType
{
    public static VType Int => new("int");
    public static VType Bool => new("bool");
    public static VType Void => new("");

    public bool Valid;
    public string Name;
    public List<VTypeMod> Mods;

    public void RemoveLastMod() => Mods.RemoveAt(Mods.Count - 1);

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

    public override string ToString() { return (Name == "" ? "void" : Name) + string.Join("", Mods.Select(mod => mod switch { 
                VFunc fn => $"({string.Join(", ", fn.Args)})", 
                VArray arr => $"[{(arr.Fixed ? arr.Size.ToString() : "")}]",
                VPointer ptr => $"@"})); }

    public bool Is<T>() where T : VTypeMod => Mods.Count > 0 && Mods.Last() is T;
    public bool Is<T>(out T mod) where T : VTypeMod 
    { mod = (T)Mods.LastOrDefault(); return Mods.Count > 0 && Mods.Last() is T; }
}
