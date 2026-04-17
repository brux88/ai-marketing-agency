namespace AiMarketingAgency.Infrastructure.Email;

public static class EmailTemplates
{
    private static string BaseLayout(string title, string body) => $"""
    <!DOCTYPE html>
    <html lang="it">
    <head>
      <meta charset="UTF-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <title>{title}</title>
    </head>
    <body style="margin:0;padding:0;background-color:#f4f4f7;font-family:Arial,Helvetica,sans-serif;">
      <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f4f7;">
        <tr>
          <td align="center" style="padding:40px 0;">
            <table role="presentation" width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);">
              <tr>
                <td style="background-color:#6366f1;padding:32px 40px;text-align:center;">
                  <h1 style="margin:0;color:#ffffff;font-size:28px;font-weight:700;">WePost AI</h1>
                </td>
              </tr>
              <tr>
                <td style="padding:40px;">
                  {body}
                </td>
              </tr>
              <tr>
                <td style="background-color:#f9fafb;padding:24px 40px;text-align:center;border-top:1px solid #e5e7eb;">
                  <p style="margin:0;color:#9ca3af;font-size:12px;">&copy; {DateTime.UtcNow.Year} WePost AI. Tutti i diritti riservati.</p>
                  <p style="margin:8px 0 0;color:#9ca3af;font-size:12px;">
                    <a href="https://www.wepostai.com" style="color:#6366f1;text-decoration:none;">www.wepostai.com</a>
                  </p>
                </td>
              </tr>
            </table>
          </td>
        </tr>
      </table>
    </body>
    </html>
    """;

    public static string EmailConfirmation(string fullName, string confirmationLink) => BaseLayout(
        "Conferma il tuo indirizzo email",
        $"""
        <h2 style="margin:0 0 16px;color:#1f2937;font-size:22px;">Ciao {fullName},</h2>
        <p style="color:#4b5563;font-size:16px;line-height:1.6;">
          Grazie per esserti registrato su <strong>WePost AI</strong>! Per completare la registrazione, conferma il tuo indirizzo email cliccando il pulsante qui sotto.
        </p>
        <div style="text-align:center;margin:32px 0;">
          <a href="{confirmationLink}" style="display:inline-block;background-color:#6366f1;color:#ffffff;text-decoration:none;padding:14px 40px;border-radius:8px;font-size:16px;font-weight:600;">
            Conferma Email
          </a>
        </div>
        <p style="color:#6b7280;font-size:14px;line-height:1.5;">
          Se non riesci a cliccare il pulsante, copia e incolla questo link nel tuo browser:<br/>
          <a href="{confirmationLink}" style="color:#6366f1;word-break:break-all;">{confirmationLink}</a>
        </p>
        <p style="color:#9ca3af;font-size:13px;margin-top:24px;">Se non hai creato un account su WePost AI, puoi ignorare questa email.</p>
        """);

    public static string PasswordReset(string fullName, string resetLink) => BaseLayout(
        "Reimposta la tua password",
        $"""
        <h2 style="margin:0 0 16px;color:#1f2937;font-size:22px;">Ciao {fullName},</h2>
        <p style="color:#4b5563;font-size:16px;line-height:1.6;">
          Abbiamo ricevuto una richiesta per reimpostare la password del tuo account <strong>WePost AI</strong>. Clicca il pulsante qui sotto per procedere.
        </p>
        <div style="text-align:center;margin:32px 0;">
          <a href="{resetLink}" style="display:inline-block;background-color:#6366f1;color:#ffffff;text-decoration:none;padding:14px 40px;border-radius:8px;font-size:16px;font-weight:600;">
            Reimposta Password
          </a>
        </div>
        <p style="color:#6b7280;font-size:14px;line-height:1.5;">
          Se non riesci a cliccare il pulsante, copia e incolla questo link nel tuo browser:<br/>
          <a href="{resetLink}" style="color:#6366f1;word-break:break-all;">{resetLink}</a>
        </p>
        <p style="color:#ef4444;font-size:14px;margin-top:16px;">
          <strong>Questo link scadr&agrave; tra 1 ora.</strong>
        </p>
        <p style="color:#9ca3af;font-size:13px;margin-top:24px;">Se non hai richiesto il reset della password, puoi ignorare questa email. La tua password non verr&agrave; modificata.</p>
        """);

    public static string Welcome(string fullName) => BaseLayout(
        "Benvenuto su WePost AI!",
        $"""
        <h2 style="margin:0 0 16px;color:#1f2937;font-size:22px;">Benvenuto {fullName}! 🎉</h2>
        <p style="color:#4b5563;font-size:16px;line-height:1.6;">
          Il tuo account &egrave; stato confermato con successo. Ora puoi accedere a tutte le funzionalit&agrave; di <strong>WePost AI</strong>.
        </p>
        <div style="margin:24px 0;padding:20px;background-color:#f0fdf4;border-radius:8px;border-left:4px solid #22c55e;">
          <p style="margin:0;color:#166534;font-size:15px;font-weight:600;">Cosa puoi fare ora:</p>
          <ul style="color:#4b5563;font-size:14px;line-height:1.8;margin:8px 0 0;padding-left:20px;">
            <li>Crea la tua prima agenzia di marketing</li>
            <li>Collega i tuoi canali social</li>
            <li>Genera contenuti con l'AI</li>
            <li>Pianifica e pubblica automaticamente</li>
          </ul>
        </div>
        <div style="text-align:center;margin:32px 0;">
          <a href="https://www.wepostai.com/dashboard" style="display:inline-block;background-color:#6366f1;color:#ffffff;text-decoration:none;padding:14px 40px;border-radius:8px;font-size:16px;font-weight:600;">
            Vai alla Dashboard
          </a>
        </div>
        """);

    public static string AccountDeletionConfirmation(string fullName, string confirmationLink) => BaseLayout(
        "Conferma eliminazione account",
        $"""
        <h2 style="margin:0 0 16px;color:#1f2937;font-size:22px;">Ciao {fullName},</h2>
        <p style="color:#4b5563;font-size:16px;line-height:1.6;">
          Abbiamo ricevuto una richiesta per eliminare il tuo account <strong>WePost AI</strong>. Questa azione &egrave; <strong>irreversibile</strong> e comporta la cancellazione di tutti i tuoi dati.
        </p>
        <div style="margin:24px 0;padding:16px;background-color:#fef2f2;border-radius:8px;border-left:4px solid #ef4444;">
          <p style="margin:0;color:#991b1b;font-size:14px;">
            <strong>Attenzione:</strong> Tutti i tuoi dati, agenzie, progetti, contenuti e connessioni social verranno eliminati definitivamente.
          </p>
        </div>
        <div style="text-align:center;margin:32px 0;">
          <a href="{confirmationLink}" style="display:inline-block;background-color:#ef4444;color:#ffffff;text-decoration:none;padding:14px 40px;border-radius:8px;font-size:16px;font-weight:600;">
            Conferma Eliminazione
          </a>
        </div>
        <p style="color:#6b7280;font-size:14px;line-height:1.5;">
          Se non riesci a cliccare il pulsante, copia e incolla questo link nel tuo browser:<br/>
          <a href="{confirmationLink}" style="color:#ef4444;word-break:break-all;">{confirmationLink}</a>
        </p>
        <p style="color:#ef4444;font-size:14px;margin-top:16px;">
          <strong>Questo link scadr&agrave; tra 1 ora.</strong>
        </p>
        <p style="color:#9ca3af;font-size:13px;margin-top:24px;">Se non hai richiesto l'eliminazione del tuo account, puoi ignorare questa email. Il tuo account rester&agrave; attivo.</p>
        """);

    public static string TeamInvitation(string inviterName, string teamName, string invitationLink) => BaseLayout(
        "Invito al team - WePost AI",
        $"""
        <h2 style="margin:0 0 16px;color:#1f2937;font-size:22px;">Sei stato invitato!</h2>
        <p style="color:#4b5563;font-size:16px;line-height:1.6;">
          <strong>{inviterName}</strong> ti ha invitato a far parte del team <strong>{teamName}</strong> su <strong>WePost AI</strong>.
        </p>
        <div style="text-align:center;margin:32px 0;">
          <a href="{invitationLink}" style="display:inline-block;background-color:#6366f1;color:#ffffff;text-decoration:none;padding:14px 40px;border-radius:8px;font-size:16px;font-weight:600;">
            Accetta Invito
          </a>
        </div>
        <p style="color:#6b7280;font-size:14px;line-height:1.5;">
          Se non riesci a cliccare il pulsante, copia e incolla questo link nel tuo browser:<br/>
          <a href="{invitationLink}" style="color:#6366f1;word-break:break-all;">{invitationLink}</a>
        </p>
        <p style="color:#9ca3af;font-size:13px;margin-top:24px;">Questo invito scadr&agrave; tra 7 giorni. Se non conosci il mittente, puoi ignorare questa email.</p>
        """);
}
