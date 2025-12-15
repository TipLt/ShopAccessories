namespace BL5_PRN212_MustPass_Project.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IAuthService _authService;

    public AuthorizationService(IAuthService authService)
    {
        _authService = authService;
    }

    public bool IsAdmin() => _authService.CurrentUser?.Role == "Admin";
    public bool IsStaff() => _authService.CurrentUser?.Role == "Staff";
    public bool IsCustomer() => _authService.CurrentUser?.Role == "Customer";

    public bool CanCreate(string module)
    {
        if (IsAdmin()) return true;
        if (IsStaff())
        {
            return module == "Customers" || module == "Orders";
        }
        return false;
    }

    public bool CanUpdate(string module)
    {
        if (IsAdmin()) return true;
        return false;
    }

    public bool CanDelete(string module)
    {
        if (IsAdmin()) return true;
        return false;
    }

    public void EnsureCanCreate(string module)
    {
        if (!CanCreate(module))
            throw new UnauthorizedAccessException($"You do not have permission to create {module}");
    }

    public void EnsureCanRead(string module)
    {
        // All authenticated users can read
        if (!_authService.IsAuthenticated)
            throw new UnauthorizedAccessException("You must be logged in");

        // Customer cannot access Users module
        if (IsCustomer() && module == "Users")
            throw new UnauthorizedAccessException($"You do not have permission to access {module}");
    }

    public void EnsureCanUpdate(string module)
    {
        if (!CanUpdate(module))
            throw new UnauthorizedAccessException($"You do not have permission to update {module}");
    }

    public void EnsureCanDelete(string module)
    {
        if (!CanDelete(module))
            throw new UnauthorizedAccessException($"You do not have permission to delete {module}");
    }

    public void EnsureCustomerSelf(int customerId)
    {
        if (IsCustomer() && _authService.CurrentUser?.CustomerId != customerId)
            throw new UnauthorizedAccessException("You can only access your own data");
    }
}
