namespace WalletApi.Extensions;

public static class ApplicationConfigurationExtension
{
    public static void ConfigureApplication(IConfiguration configuration)
    {
        ConfigureDapper();
    }

    private static void ConfigureDapper()
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    }
}