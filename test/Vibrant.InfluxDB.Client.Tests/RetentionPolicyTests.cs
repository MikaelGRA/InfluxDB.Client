using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Vibrant.InfluxDB.Client.Tests
{
    [Collection("InfluxClient collection")]
    public class RetentionPolicyTests
    {
        private const string RpName = "mypolicy";
        private const string MeasurementName = "computer_infos_rp";

        private InfluxClient _client;

        public RetentionPolicyTests(InfluxClientFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task Should_Write_And_Read_Data_From_Non_Default_RP()
        {

            _client.CreateRetentionPolicyAsync(InfluxClientFixture.DatabaseName, RpName, "1h", 1, false).Wait();
            var rps = await _client.ShowRetentionPoliciesAsync(InfluxClientFixture.DatabaseName);

            Assert.True(rps.Succeeded);
            Assert.Equal(1, rps.Series.Count);

            var rpSeries = rps.Series[0];
            Assert.Equal(2, rpSeries.Rows.Count);

            var policy = rpSeries.Rows.FirstOrDefault(row => row.Name == RpName);
            Assert.NotNull(policy);
            Assert.Equal("1h0m0s", policy.Duration);
            



            var infos = InfluxClientFixture.CreateTypedRowsStartingAt(DateTime.Now.AddHours(-2), 2*60*60, false);

            var options = new InfluxWriteOptions
            {
                RetentionPolicy = RpName
            };

            _client.WriteAsync(InfluxClientFixture.DatabaseName, MeasurementName, infos, options).Wait();
            

            var resultSet = await _client.ReadAsync<ComputerInfo>(InfluxClientFixture.DatabaseName, $"SELECT * FROM {RpName}.{MeasurementName}");
            Assert.Equal(1, resultSet.Results.Count);

            var result = resultSet.Results[0];
            Assert.Equal(1, result.Series.Count);

            var series = result.Series[0];
            Assert.Equal(2*60*60, series.Rows.Count);
        }
    }
}
