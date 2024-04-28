using System.Collections.Generic;
using System.Linq;

namespace EdComp;

public enum VTypeMod
{
    Array, Fn
}

// FUCK Equals() and GetHashCode(), all my homies FUCKING HATE Equals() and GetHashCode()
public class VType
{
    public static VType Int => new("int");

    public bool Valid;
    public string Name;
    public List<VTypeMod> Mods;

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
    public override string ToString() { return Name; }
}
