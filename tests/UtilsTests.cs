using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class MinutesTimeSpanTest
{
  [TestMethod]
  [DataRow(0, 0)]
  [DataRow(60, 60)]
  [DataRow(60, 61)]
  [DataRow(60, 119)]
  [DataRow(2 * 24 * 60 * 60, 2 * 24 * 60 * 60 + 6)]
  public void Equal(int seconds0, int seconds1)
  {
    MinutesTimeSpan a = new(TimeSpan.FromSeconds(seconds0));
    MinutesTimeSpan b = new(TimeSpan.FromSeconds(seconds1));

    Assert.AreEqual(a, b);
  }

  [TestMethod]
  public void DefaultEqual()
  {
    Assert.AreEqual(new MinutesTimeSpan(), new MinutesTimeSpan());
  }

  [TestMethod]
  [DataRow(0, 60)]
  [DataRow(59, 60)]
  [DataRow(60, 120)]
  [DataRow(1 * 24 * 60 * 60, 2 * 24 * 60 * 60)]
  public void NotEqual(int seconds0, int seconds1)
  {
    MinutesTimeSpan a = new(TimeSpan.FromSeconds(seconds0));
    MinutesTimeSpan b = new(TimeSpan.FromSeconds(seconds1));

    Assert.AreNotEqual(a, b);
  }
}
