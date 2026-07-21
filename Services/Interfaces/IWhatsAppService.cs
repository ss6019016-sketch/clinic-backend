namespace clinic.Services.Interfaces
{
    public interface IWhatsAppService
    {
        /// <summary>
        /// Sends an appointment reminder via WhatsApp using a pre-approved template.
        /// Phone must be in international format without '+' (e.g. 923001234567).
        /// </summary>
        Task<bool> SendAppointmentReminderAsync(
            string phone, string patientName, string doctorName,
            DateTime date, string time);
    }
}