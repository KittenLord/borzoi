using System.Collections.Generic;
using System.Linq;

namespace EdComp;

public interface VTypeMod 
{
    public static VFunc Fn(params VType[] args) => new VFunc() { Args = args.ToList() };
    public static VFunc Fn(IEnumerable<VType> args) => new VFunc() { Args = args.ToList() };
}

public class VFunc : VTypeMod 
{
    public List<VType> Args = new();

    public override string ToString() { return "(" + string.Join(", ", Args) + ")"; }
}

public class VArray : VTypeMod
{

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

    public static bool operator ==(VType l, VType r) => l.Name == r.Name && l.Mods.SequenceEqual(r.Mods);
    public static bool operator !=(VType l, VType r) => !(l == r);

    // TODO: parens & brackets
    public override string ToString() { return (Name == "" ? "void" : Name) + string.Join("", Mods.Select(mod => mod switch { 
                VFunc fn => $"({string.Join(", ", fn.Args)})", 
                VArray arr => "[]" })); }
}
