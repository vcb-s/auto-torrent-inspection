namespace NChardet
{
    /// <summary>
    /// Description of ICharsetDetectionObserver.
    /// </summary>
    public interface ICharsetDetectionObserver
    {
        void Notify(string charset) ;
    }
}
