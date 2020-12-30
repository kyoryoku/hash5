namespace WpfApp1
{
    /// <summary>
    /// Закрыть диалог через кнопку "ДА"
    /// </summary>
    public delegate void AcceptDialogHandler();

    /// <summary>
    /// Закрыть диалог через кнопку "НЕТ"
    /// </summary>
    public delegate void CancelDialogHandler();

    /// <summary>
    /// Открыть диалог выбора папки
    /// </summary>
    public delegate void OpenFoldeDialogHandler();

    /// <summary>
    /// Открыть диалог выбора папки
    /// </summary>
    public delegate void OpenFileDialogHandler();

}