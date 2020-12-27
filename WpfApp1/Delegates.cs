namespace WpfApp1
{
    /// <summary>
    /// Делегат закрытия диалога
    /// </summary>
    public delegate void CloseDialogHandler();

    /// <summary>
    /// Закрыть диалог через кнопку "ДА"
    /// </summary>
    public delegate void AcceptDialogHandler();

    /// <summary>
    /// Закрыть диалог через кнопку "НЕТ"
    /// </summary>
    public delegate void CancelDialogHandler();

}