using System.Globalization;

namespace GenshinImpact.MonsterMap.Domain;

public readonly struct AppFloat : IEquatable<AppFloat>
{
    private readonly float _value;

    public AppFloat(float value)
    {
        _value = value;
    }

    public AppFloat(string value)
        : this(ParsePoint(value))
    {
    }
    
    public bool Equals(AppFloat other)
    {
        const double epsilon = 0.00001;
        return Math.Abs(_value - other._value) < epsilon;
    }

    public override bool Equals(object? obj)
    {
        return obj is AppFloat other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString() => AsString(_value);

    public static implicit operator string(AppFloat value) => value.ToString();
    
    public static implicit operator float(AppFloat value) => value._value;
    
    public static implicit operator AppFloat(float value) => new(value);

    private static float ParsePoint(string value) => float.Parse(value, CultureInfo.InvariantCulture.NumberFormat);

    private static string AsString(float value) => value.ToString(CultureInfo.InvariantCulture);
}