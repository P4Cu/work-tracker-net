using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DayRecordTest
{
    [TestMethod]
    [DataRow("", "")]
    [DataRow("123", "123")]
    public void EqualOriginalInput(string originalInput0, string originalInput1)
    {
        DayRecord a = new()
        {
            TotalWorkTime = new(),
            TotalInterruptionTime = new(),
            OriginalInput = originalInput0,
        };
        DayRecord b = new()
        {
            TotalWorkTime = new(),
            TotalInterruptionTime = new(),
            OriginalInput = originalInput1,
        };
        Assert.AreEqual(a, b);
    }

    [TestMethod]
    [DataRow("", "a")]
    [DataRow("123", "124")]
    public void NotEqualOriginalInput(string originalInput0, string originalInput1)
    {
        DayRecord a = new()
        {
            TotalWorkTime = new(),
            TotalInterruptionTime = new(),
            OriginalInput = originalInput0,
        };
        DayRecord b = new()
        {
            TotalWorkTime = new(),
            TotalInterruptionTime = new(),
            OriginalInput = originalInput1,
        };
        Assert.AreNotEqual(a, b);
    }
}
