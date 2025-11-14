namespace CoreBanking.Core.ValueObjects;
public record ContactInfo
{
    public string Email { get; }
    public string PhoneNumber { get; }
    public Address Address { get; }

    public ContactInfo() {}

    public ContactInfo(string email, string phoneNumber, Address address)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        Email = email.Trim();
        PhoneNumber = phoneNumber.Trim();
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }

    public ContactInfo WithEmail(string newEmail)
    {
        return new ContactInfo(newEmail, PhoneNumber, Address);
    }

    public ContactInfo WithPhoneNumber(string newPhoneNumber)
    {
        return new ContactInfo(Email, newPhoneNumber, Address);
    }

    public ContactInfo WithAddress(Address newAddress)
    {
        return new ContactInfo(Email, PhoneNumber, newAddress);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}


