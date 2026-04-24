namespace CapstoneRegistration.API.Common;

public static class SemesterHelper
{
    public static string ComputeId(DateOnly date)
    {
        var yy = date.Year % 100;

        var prefix = date.Month switch
        {
            <= 4 => "SP",
            <= 8 => "SU",
            _    => "FA"
        };

        return $"{prefix}{yy:D2}";
    }
}
