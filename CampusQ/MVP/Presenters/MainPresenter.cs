using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using CampusQ.MVP.Views;

namespace CampusQ.MVP.Presenters
{
    public class MainPresenter
    {
        private readonly IMainView _view;
        public MainPresenter(IMainView view)
        {
            _view = view;
        }

        // Accept explicit service string from UI (less error-prone than two booleans)
        public void Submit(string purposeText, string service)
        {
            var purposePattern = new Regex(@"^[A-Za-z \&\-]{2,60}$", RegexOptions.Compiled);

            if (!purposePattern.IsMatch(purposeText))
            {
                _view.ShowMessage("Please select a valid purpose from the list.", "Validation Error", MessageBoxIcon.Warning);
                return;
            }

            var svc = NormalizeService(service);

            _view.AddToQueue(purposeText, svc);
            _view.ShowMessage("Thank you for using CampusQ", "", MessageBoxIcon.Information);
        }

        private static string NormalizeService(string? service)
        {
            if (string.IsNullOrWhiteSpace(service))
                return "Other";

            var s = service.Trim();
            var lower = s.ToLowerInvariant();

            if (lower.Contains("cash") || lower.Contains("window") || Regex.IsMatch(lower, @"(^|\s)w\s*\d", RegexOptions.IgnoreCase))
                return "Cashier";

            if (lower.Contains("registr") || lower.Equals("reg", StringComparison.OrdinalIgnoreCase))
                return "Registrar";

            if (string.Equals(s, "other", StringComparison.OrdinalIgnoreCase))
                return "Other";

            // Fallback: title-case the provided value
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());
        }
    }
}
