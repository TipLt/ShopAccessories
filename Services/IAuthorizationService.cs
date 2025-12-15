namespace BL5_PRN212_MustPass_Project.Services;

public interface IAuthorizationService
{
    bool IsAdmin();
    bool IsStaff();
    bool IsCustomer();
    void EnsureCanCreate(string module);
    void EnsureCanRead(string module);
    void EnsureCanUpdate(string module);
    void EnsureCanDelete(string module);
    void EnsureCustomerSelf(int customerId);
    bool CanCreate(string module);
    bool CanUpdate(string module);
    bool CanDelete(string module);
}
