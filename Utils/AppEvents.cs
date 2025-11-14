using System;

public static class AppEvents
{
    /// <summary>
    /// Disparado sempre que configurações (tema, idioma etc) forem alteradas
    /// </summary>
    public static event Action OnSettingsChanged;

    /// <summary>
    /// Dispara o evento para notificar todas as telas que o app foi atualizado
    /// </summary>
    public static void RaiseSettingsChanged()
    {
        OnSettingsChanged?.Invoke();
    }

    // =================== NOVO ===================

    /// <summary>
    /// Disparado sempre que há uma nova notificação para o usuário
    /// </summary>
    public static event Action OnNotificationChanged;

    /// <summary>
    /// Chamar sempre que uma notificação nova for adicionada
    /// </summary>
    public static void NotifyDashboardToUpdateNotifications()
    {
        OnNotificationChanged?.Invoke();
    }
}
