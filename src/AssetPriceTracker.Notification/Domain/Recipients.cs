using System.Collections.Generic;

// Deprecated
namespace AssetPriceTracker.Notification.Domain
{
    public class Recipients
    {
        public List<string> EmailAddresses { get; private set; }

        public Recipients()
        {
            EmailAddresses = new List<string>();
        }
        public void AddRecipient(string email)
        {
            if (!string.IsNullOrWhiteSpace(email) && !EmailAddresses.Contains(email))
            {
                EmailAddresses.Add(email);
            }
        }

        public void RemoveRecipient(string email)
        {
            EmailAddresses.Remove(email);
        }

        public void ClearRecipients()
        {
            EmailAddresses.Clear();
        }
    }
}
