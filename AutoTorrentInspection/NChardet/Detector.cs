namespace NChardet
{
    public class Detector : PSMDetector, ICharsetDetector
    {
        ICharsetDetectionObserver mObserver = null;

        public Detector()
        {
            //base();
        }

        public Detector(int langFlag)
        {
            //base(langFlag);
        }

        public void Init(ICharsetDetectionObserver aObserver)
        {
            mObserver = aObserver;
        }

        public bool DoIt(byte[] aBuf, int aLen, bool oDontFeedMe)
        {
            if (aBuf == null || oDontFeedMe)
                return false;

            HandleData(aBuf, aLen);
            return mDone;
        }

        public void Done()
        {
            DataEnd();
        }

        public override void Report(string charset)
        {
            mObserver?.Notify(charset);
        }

        public static bool isAscii(byte[] aBuf, int aLen)
        {
            for (int i = 0; i < aLen; i++)
            {
                if ((0x0080 & aBuf[i]) != 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
