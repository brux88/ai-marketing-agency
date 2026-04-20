namespace AiMarketingAgency.Application.Email;

public static class EmailTemplates
{
    // wepostai brand palette — mirrors frontend/src/app/globals.css tokens.
    private const string Ink = "#222222";
    private const string Paper = "#FBFAF5";
    private const string Paper2 = "#F2EFE8";
    private const string Line = "#E4E1DA";
    private const string MutedInk = "#898780";
    private const string Lime = "#BFE836";
    private const string LimeDeep = "#86B224";
    private const string Terra = "#C07F4E";
    private const string ErrorColor = "#CD3A3A";

    private static string Header() => $"""
        <td style="background-color:{Ink};padding:32px 40px;text-align:center;">
          <table role="presentation" cellpadding="0" cellspacing="0" style="margin:0 auto;">
            <tr>
              <td style="vertical-align:middle;">
                <span style="display:inline-block;width:36px;height:36px;background-color:{Ink};border:2px solid {Paper};border-radius:8px;text-align:center;line-height:32px;color:{Paper};font-size:22px;font-weight:500;">w</span>
              </td>
              <td style="vertical-align:middle;padding-left:10px;">
                <span style="color:{Paper};font-size:26px;font-weight:500;letter-spacing:-0.8px;">wepost</span><span style="color:{MutedInk};font-size:26px;font-weight:500;font-style:italic;letter-spacing:-0.8px;">ai.com</span>
              </td>
            </tr>
          </table>
        </td>
        """;

    private static string BaseLayout(string title, string body, string? footerExtra = null) => $"""
    <!DOCTYPE html>
    <html lang="it">
    <head>
      <meta charset="UTF-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <title>{title}</title>
    </head>
    <body style="margin:0;padding:0;background-color:{Paper2};font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Helvetica,Arial,sans-serif;">
      <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:{Paper2};">
        <tr>
          <td align="center" style="padding:40px 16px;">
            <table role="presentation" width="600" cellpadding="0" cellspacing="0" style="max-width:600px;background-color:{Paper};border:1px solid {Line};border-radius:14px;overflow:hidden;">
              <tr>
                {Header()}
              </tr>
              <tr>
                <td style="padding:40px;">
                  {body}
                </td>
              </tr>
              <tr>
                <td style="background-color:{Paper2};padding:20px 40px;text-align:center;border-top:1px solid {Line};">
                  <p style="margin:0;color:{MutedInk};font-size:12px;letter-spacing:-0.1px;">&copy; {DateTime.UtcNow.Year} wepostai.com &mdash; Tutti i diritti riservati.</p>
                  <p style="margin:6px 0 0;color:{MutedInk};font-size:12px;">
                    <a href="https://www.wepostai.com" style="color:{Ink};text-decoration:none;font-weight:600;">www.wepostai.com</a>
                  </p>
                  {footerExtra ?? string.Empty}
                </td>
              </tr>
            </table>
          </td>
        </tr>
      </table>
    </body>
    </html>
    """;

    private static string PrimaryButton(string href, string label) => $"""
        <div style="text-align:center;margin:32px 0;">
          <a href="{href}" style="display:inline-block;background-color:{Ink};color:{Paper};text-decoration:none;padding:14px 36px;border-radius:10px;font-size:16px;font-weight:600;letter-spacing:-0.1px;">
            {label}
          </a>
        </div>
        """;

    private static string DangerButton(string href, string label) => $"""
        <div style="text-align:center;margin:32px 0;">
          <a href="{href}" style="display:inline-block;background-color:{ErrorColor};color:{Paper};text-decoration:none;padding:14px 36px;border-radius:10px;font-size:16px;font-weight:600;letter-spacing:-0.1px;">
            {label}
          </a>
        </div>
        """;

    public static string EmailConfirmation(string fullName, string confirmationLink) => BaseLayout(
        "Conferma il tuo indirizzo email",
        $"""
        <h2 style="margin:0 0 16px;color:{Ink};font-size:22px;letter-spacing:-0.3px;">Ciao {fullName},</h2>
        <p style="color:{Ink};font-size:16px;line-height:1.6;">
          Grazie per esserti registrato su <strong>wepostai.com</strong>. Per completare la registrazione, conferma il tuo indirizzo email cliccando il pulsante qui sotto.
        </p>
        {PrimaryButton(confirmationLink, "Conferma Email")}
        <p style="color:{MutedInk};font-size:14px;line-height:1.5;">
          Se non riesci a cliccare il pulsante, copia e incolla questo link nel tuo browser:<br/>
          <a href="{confirmationLink}" style="color:{Ink};word-break:break-all;">{confirmationLink}</a>
        </p>
        <p style="color:{MutedInk};font-size:13px;margin-top:24px;">Se non hai creato un account su wepostai.com, puoi ignorare questa email.</p>
        """);

    public static string PasswordReset(string fullName, string resetLink) => BaseLayout(
        "Reimposta la tua password",
        $"""
        <h2 style="margin:0 0 16px;color:{Ink};font-size:22px;letter-spacing:-0.3px;">Ciao {fullName},</h2>
        <p style="color:{Ink};font-size:16px;line-height:1.6;">
          Abbiamo ricevuto una richiesta per reimpostare la password del tuo account <strong>wepostai.com</strong>. Clicca il pulsante qui sotto per procedere.
        </p>
        {PrimaryButton(resetLink, "Reimposta Password")}
        <p style="color:{MutedInk};font-size:14px;line-height:1.5;">
          Se non riesci a cliccare il pulsante, copia e incolla questo link nel tuo browser:<br/>
          <a href="{resetLink}" style="color:{Ink};word-break:break-all;">{resetLink}</a>
        </p>
        <p style="color:{ErrorColor};font-size:14px;margin-top:16px;">
          <strong>Questo link scadr&agrave; tra 1 ora.</strong>
        </p>
        <p style="color:{MutedInk};font-size:13px;margin-top:24px;">Se non hai richiesto il reset della password, puoi ignorare questa email. La tua password non verr&agrave; modificata.</p>
        """);

    public static string Welcome(string fullName) => BaseLayout(
        "Benvenuto su wepostai.com",
        $"""
        <h2 style="margin:0 0 16px;color:{Ink};font-size:22px;letter-spacing:-0.3px;">Benvenuto {fullName}</h2>
        <p style="color:{Ink};font-size:16px;line-height:1.6;">
          Il tuo account &egrave; stato confermato con successo. Ora puoi accedere a tutte le funzionalit&agrave; di <strong>wepostai.com</strong>.
        </p>
        <div style="margin:24px 0;padding:20px;background-color:{Paper2};border-radius:12px;border-left:4px solid {LimeDeep};">
          <p style="margin:0;color:{Ink};font-size:15px;font-weight:600;">Cosa puoi fare ora</p>
          <ul style="color:{Ink};font-size:14px;line-height:1.8;margin:8px 0 0;padding-left:20px;">
            <li>Crea la tua prima agenzia di marketing</li>
            <li>Collega i tuoi canali social</li>
            <li>Genera contenuti con l'AI</li>
            <li>Pianifica e pubblica automaticamente</li>
          </ul>
        </div>
        {PrimaryButton("https://www.wepostai.com/dashboard", "Vai alla Dashboard")}
        """);

    public static string AccountDeletionConfirmation(string fullName, string confirmationLink) => BaseLayout(
        "Conferma eliminazione account",
        $"""
        <h2 style="margin:0 0 16px;color:{Ink};font-size:22px;letter-spacing:-0.3px;">Ciao {fullName},</h2>
        <p style="color:{Ink};font-size:16px;line-height:1.6;">
          Abbiamo ricevuto una richiesta per eliminare il tuo account <strong>wepostai.com</strong>. Questa azione &egrave; <strong>irreversibile</strong> e comporta la cancellazione di tutti i tuoi dati.
        </p>
        <div style="margin:24px 0;padding:16px;background-color:#FBE5E5;border-radius:10px;border-left:4px solid {ErrorColor};">
          <p style="margin:0;color:{ErrorColor};font-size:14px;">
            <strong>Attenzione:</strong> Tutti i tuoi dati, agenzie, progetti, contenuti e connessioni social verranno eliminati definitivamente.
          </p>
        </div>
        {DangerButton(confirmationLink, "Conferma Eliminazione")}
        <p style="color:{MutedInk};font-size:14px;line-height:1.5;">
          Se non riesci a cliccare il pulsante, copia e incolla questo link nel tuo browser:<br/>
          <a href="{confirmationLink}" style="color:{ErrorColor};word-break:break-all;">{confirmationLink}</a>
        </p>
        <p style="color:{ErrorColor};font-size:14px;margin-top:16px;">
          <strong>Questo link scadr&agrave; tra 1 ora.</strong>
        </p>
        <p style="color:{MutedInk};font-size:13px;margin-top:24px;">Se non hai richiesto l'eliminazione del tuo account, puoi ignorare questa email. Il tuo account rester&agrave; attivo.</p>
        """);

    public static string TeamInvitation(string inviterName, string teamName, string invitationLink) => BaseLayout(
        "Invito al team - wepostai.com",
        $"""
        <h2 style="margin:0 0 16px;color:{Ink};font-size:22px;letter-spacing:-0.3px;">Sei stato invitato</h2>
        <p style="color:{Ink};font-size:16px;line-height:1.6;">
          <strong>{inviterName}</strong> ti ha invitato a far parte del team <strong>{teamName}</strong> su <strong>wepostai.com</strong>.
        </p>
        {PrimaryButton(invitationLink, "Accetta Invito")}
        <p style="color:{MutedInk};font-size:14px;line-height:1.5;">
          Se non riesci a cliccare il pulsante, copia e incolla questo link nel tuo browser:<br/>
          <a href="{invitationLink}" style="color:{Ink};word-break:break-all;">{invitationLink}</a>
        </p>
        <p style="color:{MutedInk};font-size:13px;margin-top:24px;">Questo invito scadr&agrave; tra 7 giorni. Se non conosci il mittente, puoi ignorare questa email.</p>
        """);

    /// Newsletter email for subscribers. The body HTML is content generated by
    /// the agency/project. We wrap it in the brand layout and append an
    /// unsubscribe footer with a one-click link.
    public static string Newsletter(string subject, string bodyHtml, string unsubscribeLink) => BaseLayout(
        subject,
        $"""
        <div style="color:{Ink};font-size:16px;line-height:1.65;">
          {bodyHtml}
        </div>
        """,
        footerExtra: $"""
          <p style="margin:10px 0 0;color:{MutedInk};font-size:12px;">
            Non vuoi pi&ugrave; ricevere queste email?
            <a href="{unsubscribeLink}" style="color:{Ink};text-decoration:underline;font-weight:600;">Disiscriviti</a>
          </p>
        """);

    /// Notification sent to the agency/project owner when someone subscribes
    /// to the newsletter.
    public static string NewSubscriberNotification(string subscriberEmail, string targetName, string? dashboardLink = null) => BaseLayout(
        "Nuovo iscritto alla newsletter",
        $"""
        <h2 style="margin:0 0 16px;color:{Ink};font-size:22px;letter-spacing:-0.3px;">Nuovo iscritto</h2>
        <p style="color:{Ink};font-size:16px;line-height:1.6;">
          <strong>{subscriberEmail}</strong> si &egrave; appena iscritto alla newsletter di <strong>{targetName}</strong>.
        </p>
        {(string.IsNullOrEmpty(dashboardLink) ? string.Empty : PrimaryButton(dashboardLink, "Apri dashboard"))}
        """);
}
