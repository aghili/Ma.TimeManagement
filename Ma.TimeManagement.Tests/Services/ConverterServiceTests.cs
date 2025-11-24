namespace Ma.TimeManagement.Services.Tests
{
    [TestClass()]
    public class ConverterServiceTests
    {
        [TestMethod()]
        public void ConvertHourToRoundedTest()
        {
            ConverterService srv = new();
            Assert.AreEqual(1.25, srv.ConvertHourToRounded(1.12), 0);
            Assert.AreEqual(1.50, srv.ConvertHourToRounded(1.30), 0);
            Assert.AreEqual(1.75, srv.ConvertHourToRounded(1.60), 0);
            Assert.AreEqual(2, srv.ConvertHourToRounded(1.9), 0);
        }
    }
}