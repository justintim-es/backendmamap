namespace latest.Email;

public interface IEmailSender {
    void SendEmail(EmailMessage message);
}
