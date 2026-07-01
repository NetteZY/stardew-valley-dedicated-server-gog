using JunimoServer.Tests.Clients;
using JunimoServer.Tests.Helpers;
using JunimoServer.Tests.Infrastructure;
using Xunit;

namespace JunimoServer.Tests;

/// <summary>
/// Integration tests for server navigation and joining.
/// </summary>
[TestServer(Isolation = IsolationMode.SharedAssembly)]
public class NavigationTests : TestBase
{
    public NavigationTests() { }

    /// <summary>
    /// Tests the flow of joining a server via LAN connection.
    /// </summary>
    [Fact]
    [TestServer]
    public async Task JoinServer_ViaLan_ShouldSucceed()
    {
        // Ensure disconnected
        await Connect.EnsureDisconnectedAsync();

        // Verify server is online
        Assert.NotNull(ServerStatus);
        Assert.True(ServerStatus.IsOnline, "Server should be online");

        var ct = TestContext.Current.CancellationToken;

        // Connect with retry (automatically uses LAN because WithSteam is false)
        var result = await Connect.WithRetryAsync(ct);
        Connect.AssertConnectionSuccess(result);

        Log($"Successfully joined server via LAN after {result.AttemptsUsed} attempt(s)");
        Log($"Found {result.Farmhands?.Farmhands.Count ?? 0} farmhand slots");
    }

    [Fact]
    [TestServer(Clients = 0)]
    public async Task ServerApi_GetStatus_ShouldReturnValidResponse()
    {
        var status = await ServerApi.GetStatus(TestContext.Current.CancellationToken);

        Assert.NotNull(status);
        Assert.NotNull(status.ServerVersion);
        Assert.NotNull(status.LastUpdated);

        Log($"Server version: {status.ServerVersion}");
        Log($"Online: {status.IsOnline}, Ready: {status.IsReady}");
    }

    [Fact]
    [TestServer(Clients = 0)]
    public async Task ServerApi_GetHealth_ShouldReturnOk()
    {
        // Long-poll until healthy. /wait/health uses a stateless predicate
        // (no `since` cursor — the server's health bit is "is the game thread
        // ticking now"), so each iteration returns immediately on a healthy
        // server or after up to WaitMaxTimeout (10s) on a stalled one.
        var ct = TestContext.Current.CancellationToken;
        HealthResponse? health = null;
        var healthy = await PollingHelper.LongPollAsync(
            WaitName.Polling_Navigation_HealthyOk,
            async (_, remaining) =>
            {
                health = await ServerApi.WaitForHealthAsync(
                    ready: true,
                    timeout: remaining,
                    ct: ct
                );
                // /wait/health is stateless: a 200 always satisfies the predicate,
                // a 408 means the per-tick TCS didn't fire within the server-side
                // window — re-issue without a cursor.
                return new PollingHelper.LongPollResult(health?.Status == "ok", 0);
            },
            TestTimings.ServerReadyBetweenTests,
            cancellationToken: ct
        );

        Assert.NotNull(health);
        Assert.True(
            healthy,
            $"Expected health 'ok' but got '{health.Status}' (lastTickMs={health.LastTickMs}, gameAvailable={health.GameAvailable})"
        );
        Assert.False(string.IsNullOrEmpty(health.Timestamp));

        Log($"Health status: {health.Status}");
    }
}
