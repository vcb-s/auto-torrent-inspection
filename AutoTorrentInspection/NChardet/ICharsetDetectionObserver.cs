namespace NChardet
{
    public interface ICharsetDetectionObserver
    {
        void Notify(string charset) ;
    }
}
