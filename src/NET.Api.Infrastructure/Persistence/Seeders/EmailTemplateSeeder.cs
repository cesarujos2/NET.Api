using Microsoft.EntityFrameworkCore;
using NET.Api.Domain.Entities;
using NET.Api.Shared.Constants;

namespace NET.Api.Infrastructure.Persistence.Seeders;

public static class EmailTemplateSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.EmailTemplates.AnyAsync())
            return;

        var templates = new List<EmailTemplate>
        {
            new(
                "Confirmación de Email",
                "Confirma tu dirección de correo electrónico - {{UserName}}",
                GetEmailConfirmationHtmlTemplate(),
                GetEmailConfirmationTextTemplate(),
                EmailTemplateTypes.EmailConfirmation,
                "Template para confirmar la dirección de correo electrónico del usuario",
                "owner@netapi.com"
            ),
            
            new(
                "Restablecer Contraseña",
                "Restablece tu contraseña - {{UserName}}",
                GetPasswordResetHtmlTemplate(),
                GetPasswordResetTextTemplate(),
                EmailTemplateTypes.PasswordReset,
                "Template para restablecer la contraseña del usuario",
                "owner@netapi.com"
            ),
            
            new(
                "Bienvenida",
                "¡Bienvenido a NET.Api! - {{UserName}}",
                GetWelcomeHtmlTemplate(),
                GetWelcomeTextTemplate(),
                EmailTemplateTypes.Welcome,
                "Template de bienvenida para nuevos usuarios",
                "owner@netapi.com"
            ),
            
            new(
                "Cambio de Email",
                "Confirma tu nuevo correo electrónico - {{UserName}}",
                GetEmailChangeHtmlTemplate(),
                GetEmailChangeTextTemplate(),
                EmailTemplateTypes.EmailChange,
                "Template para confirmar el cambio de dirección de correo electrónico",
                "owner@netapi.com"
            )
        };

        context.EmailTemplates.AddRange(templates);
        await context.SaveChangesAsync();
    }

    private static string GetEmailConfirmationHtmlTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Confirmación de Email</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #007bff; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .button { display: inline-block; padding: 12px 24px; background-color: #28a745; color: white; text-decoration: none; border-radius: 4px; }
        .footer { text-align: center; padding: 20px; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>¡Confirma tu Email!</h1>
        </div>
        <div class=""content"">
            <h2>Hola {{UserName}},</h2>
            <p>Gracias por registrarte en NET.Api. Para completar tu registro, necesitas confirmar tu dirección de correo electrónico.</p>
            <p>Haz clic en el siguiente botón para confirmar tu email:</p>
            <p style=""text-align: center;"">
                <a href=""{{ConfirmationUrl}}"" class=""button"">Confirmar Email</a>
            </p>
            <p>Si no puedes hacer clic en el botón, copia y pega el siguiente enlace en tu navegador:</p>
            <p>{{ConfirmationUrl}}</p>
            <p>Este enlace expirará en 24 horas por seguridad.</p>
        </div>
        <div class=""footer"">
            <p>Si no te registraste en NET.Api, puedes ignorar este email.</p>
            <p>&copy; 2024 NET.Api. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string GetEmailConfirmationTextTemplate()
    {
        return @"
Hola {{UserName}},

Gracias por registrarte en NET.Api. Para completar tu registro, necesitas confirmar tu dirección de correo electrónico.

Haz clic en el siguiente enlace para confirmar tu email:
{{ConfirmationUrl}}

Este enlace expirará en 24 horas por seguridad.

Si no te registraste en NET.Api, puedes ignorar este email.

© 2024 NET.Api. Todos los derechos reservados.";
    }

    private static string GetPasswordResetHtmlTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Restablecer Contraseña</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #dc3545; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .button { display: inline-block; padding: 12px 24px; background-color: #dc3545; color: white; text-decoration: none; border-radius: 4px; }
        .footer { text-align: center; padding: 20px; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Restablecer Contraseña</h1>
        </div>
        <div class=""content"">
            <h2>Hola {{UserName}},</h2>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta en NET.Api.</p>
            <p>Haz clic en el siguiente botón para restablecer tu contraseña:</p>
            <p style=""text-align: center;"">
                <a href=""{{ResetUrl}}"" class=""button"">Restablecer Contraseña</a>
            </p>
            <p>Si no puedes hacer clic en el botón, copia y pega el siguiente enlace en tu navegador:</p>
            <p>{{ResetUrl}}</p>
            <p>Este enlace expirará en 1 hora por seguridad.</p>
        </div>
        <div class=""footer"">
            <p>Si no solicitaste restablecer tu contraseña, puedes ignorar este email.</p>
            <p>&copy; 2024 NET.Api. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string GetPasswordResetTextTemplate()
    {
        return @"
Hola {{UserName}},

Recibimos una solicitud para restablecer la contraseña de tu cuenta en NET.Api.

Haz clic en el siguiente enlace para restablecer tu contraseña:
{{ResetUrl}}

Este enlace expirará en 1 hora por seguridad.

Si no solicitaste restablecer tu contraseña, puedes ignorar este email.

© 2024 NET.Api. Todos los derechos reservados.";
    }

    private static string GetWelcomeHtmlTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>¡Bienvenido!</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #28a745; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .button { display: inline-block; padding: 12px 24px; background-color: #007bff; color: white; text-decoration: none; border-radius: 4px; }
        .footer { text-align: center; padding: 20px; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>¡Bienvenido a NET.Api!</h1>
        </div>
        <div class=""content"">
            <h2>Hola {{UserName}},</h2>
            <p>¡Te damos la bienvenida a NET.Api! Tu cuenta ha sido creada exitosamente y ya puedes comenzar a usar nuestra plataforma.</p>
            <p>Estamos emocionados de tenerte como parte de nuestra comunidad.</p>
            <p style=""text-align: center;"">
                <a href=""https://localhost:7000"" class=""button"">Comenzar Ahora</a>
            </p>
            <p>Si tienes alguna pregunta o necesitas ayuda, no dudes en contactarnos.</p>
        </div>
        <div class=""footer"">
            <p>Gracias por elegir NET.Api</p>
            <p>&copy; 2024 NET.Api. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string GetWelcomeTextTemplate()
    {
        return @"
¡Hola {{UserName}}!

¡Te damos la bienvenida a NET.Api! Tu cuenta ha sido creada exitosamente y ya puedes comenzar a usar nuestra plataforma.

Estamos emocionados de tenerte como parte de nuestra comunidad.

Si tienes alguna pregunta o necesitas ayuda, no dudes en contactarnos.

Gracias por elegir NET.Api
© 2024 NET.Api. Todos los derechos reservados.";
    }

    private static string GetEmailChangeHtmlTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Confirma tu nuevo correo electrónico</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #007bff; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .footer { background-color: #333; color: white; padding: 10px; text-align: center; font-size: 12px; }
        .button { display: inline-block; padding: 12px 24px; background-color: #28a745; color: white; text-decoration: none; border-radius: 4px; }
        .button:hover { background-color: #218838; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Confirma tu nuevo correo electrónico</h1>
        </div>
        <div class=""content"">
            <h2>Hola {{UserName}},</h2>
            <p>Has solicitado cambiar tu dirección de correo electrónico. Para completar este cambio, necesitamos que confirmes tu nueva dirección.</p>
            <p>Haz clic en el siguiente botón para confirmar tu nuevo correo electrónico:</p>
            <p style=""text-align: center;"">
                <a href=""{{ConfirmationUrl}}"" class=""button"">Confirmar Nuevo Email</a>
            </p>
            <p>Si no solicitaste este cambio, puedes ignorar este correo. Tu dirección de correo actual permanecerá sin cambios.</p>
            <p><strong>Nota:</strong> Este enlace expirará en 24 horas por motivos de seguridad.</p>
        </div>
        <div class=""footer"">
            <p>Gracias por usar NET.Api</p>
            <p>&copy; 2024 NET.Api. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string GetEmailChangeTextTemplate()
    {
        return @"
¡Hola {{UserName}}!

Has solicitado cambiar tu dirección de correo electrónico. Para completar este cambio, necesitamos que confirmes tu nueva dirección.

Haz clic en el siguiente enlace para confirmar tu nuevo correo electrónico:
{{ConfirmationUrl}}

Si no solicitaste este cambio, puedes ignorar este correo. Tu dirección de correo actual permanecerá sin cambios.

Nota: Este enlace expirará en 24 horas por motivos de seguridad.

Gracias por usar NET.Api
© 2024 NET.Api. Todos los derechos reservados.";
    }
}