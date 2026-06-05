namespace ParkingSystem.AppCore.Authorization;

public enum AppPolicies
{
    AdminOnly,
    ActiveUser,
    OperatorOnly
}

public static class AppPoliciesExtensions
{
    public static string Name(this AppPolicies policy)
    {
        return policy.ToString();
    }
}