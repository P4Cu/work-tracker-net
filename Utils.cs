public class SimpleProp<T>
{
  public SimpleProp(T value) => _value = value;

  public event EventHandler<T>? Changed;

  private T _value;

  public T Value
  {
    get { return _value; }
    set
    {
      if (!_value!.Equals(value))
      {
        _value = value;
        Changed?.Invoke(this, _value);
      }
    }
  }

  public override string ToString() => Value!.ToString()!;
};

// Reduced precision to full minutes.
public class MinutesTimeSpan : IEquatable<MinutesTimeSpan>
{
  private TimeSpan ts;

  public MinutesTimeSpan() => ts = TimeSpan.Zero;

  public MinutesTimeSpan(TimeSpan t) => ts = t;

  public void Assign(TimeSpan t) => ts = t;

  public bool Equals(MinutesTimeSpan? other)
  {
    // TODO: make this without extensions methods.
    return ts.TotalMinutesEqual(other!.ts);
  }

  public override string ToString()
  {
    // TODO: no extension method
    return ts.ToHoursMinutesString();
  }

  public static MinutesTimeSpan operator -(MinutesTimeSpan v1, MinutesTimeSpan v2)
  {
    return new MinutesTimeSpan(v1.ts - v2.ts);
  }

  public static MinutesTimeSpan operator +(MinutesTimeSpan v1, MinutesTimeSpan v2)
  {
    return new MinutesTimeSpan(v1.ts + v2.ts);
  }
};
