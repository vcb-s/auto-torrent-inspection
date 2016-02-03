namespace NChardet
{
    public sealed class UTF8Verifier : Verifier
    {
        static int[]  _cclass   ;
        static int[]  _states   ;
        static int    _stFactor ;
        static string _charset  ;

        public override int[]  cclass()   => _cclass;
        public override int[]  states()   => _states;
        public override int    stFactor() => _stFactor;
        public override string charset()  => _charset;

        public UTF8Verifier()
        {
            _cclass = new int[256/8] ;
            _cclass[0]  = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[1]  = ((  ((  (0  << 4) | 0  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[2]  = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[3]  = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (0 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[4]  = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[5]  = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[6]  = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[7]  = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[8]  = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[9]  = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[10] = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[11] = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[12] = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[13] = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[14] = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[15] = ((  ((  (1  << 4) | 1  ) << 8) | (1 << 4) |  1 ) << 16) | ((  (1 << 4) | 1 ) << 8) | (1 << 4) | 1 ;
            _cclass[16] = ((  ((  (3  << 4) | 3  ) << 8) | (3 << 4) |  3 ) << 16) | ((  (2 << 4) | 2 ) << 8) | (2 << 4) | 2 ;
            _cclass[17] = ((  ((  (4  << 4) | 4  ) << 8) | (4 << 4) |  4 ) << 16) | ((  (4 << 4) | 4 ) << 8) | (4 << 4) | 4 ;
            _cclass[18] = ((  ((  (4  << 4) | 4  ) << 8) | (4 << 4) |  4 ) << 16) | ((  (4 << 4) | 4 ) << 8) | (4 << 4) | 4 ;
            _cclass[19] = ((  ((  (4  << 4) | 4  ) << 8) | (4 << 4) |  4 ) << 16) | ((  (4 << 4) | 4 ) << 8) | (4 << 4) | 4 ;
            _cclass[20] = ((  ((  (5  << 4) | 5  ) << 8) | (5 << 4) |  5 ) << 16) | ((  (5 << 4) | 5 ) << 8) | (5 << 4) | 5 ;
            _cclass[21] = ((  ((  (5  << 4) | 5  ) << 8) | (5 << 4) |  5 ) << 16) | ((  (5 << 4) | 5 ) << 8) | (5 << 4) | 5 ;
            _cclass[22] = ((  ((  (5  << 4) | 5  ) << 8) | (5 << 4) |  5 ) << 16) | ((  (5 << 4) | 5 ) << 8) | (5 << 4) | 5 ;
            _cclass[23] = ((  ((  (5  << 4) | 5  ) << 8) | (5 << 4) |  5 ) << 16) | ((  (5 << 4) | 5 ) << 8) | (5 << 4) | 5 ;
            _cclass[24] = ((  ((  (6  << 4) | 6  ) << 8) | (6 << 4) |  6 ) << 16) | ((  (6 << 4) | 6 ) << 8) | (0 << 4) | 0 ;
            _cclass[25] = ((  ((  (6  << 4) | 6  ) << 8) | (6 << 4) |  6 ) << 16) | ((  (6 << 4) | 6 ) << 8) | (6 << 4) | 6 ;
            _cclass[26] = ((  ((  (6  << 4) | 6  ) << 8) | (6 << 4) |  6 ) << 16) | ((  (6 << 4) | 6 ) << 8) | (6 << 4) | 6 ;
            _cclass[27] = ((  ((  (6  << 4) | 6  ) << 8) | (6 << 4) |  6 ) << 16) | ((  (6 << 4) | 6 ) << 8) | (6 << 4) | 6 ;
            _cclass[28] = ((  ((  (8  << 4) | 8  ) << 8) | (8 << 4) |  8 ) << 16) | ((  (8 << 4) | 8 ) << 8) | (8 << 4) | 7 ;
            _cclass[29] = ((  ((  (8  << 4) | 8  ) << 8) | (9 << 4) |  8 ) << 16) | ((  (8 << 4) | 8 ) << 8) | (8 << 4) | 8 ;
            _cclass[30] = ((  ((  (11 << 4) | 11 ) << 8) | (11<< 4) | 11 ) << 16) | ((  (11<< 4) | 11 )<< 8) | (11<< 4) | 10 ;
            _cclass[31] = ((  ((  (0  << 4) | 0  ) << 8) | (15<< 4) | 14 ) << 16) | ((  (13<< 4) | 13 )<< 8) | (13<< 4) | 12 ;

            _states = new int[26] ;
            _states[0]  = ((  ((  (10     << 4) |     12  ) << 8) | (eError << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eStart << 4) | eError ;
            _states[1]  = ((  ((  (3      << 4) |      4  ) << 8) | (5      << 4) |      6 ) << 16) | ((  (7      << 4) |     8  ) << 8) | (11     << 4) | 9 ;
            _states[2]  = ((  ((  (eError << 4) | eError  ) << 8) | (eError << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[3]  = ((  ((  (eError << 4) | eError  ) << 8) | (eError << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[4]  = ((  ((  (eItsMe << 4) | eItsMe  ) << 8) | (eItsMe << 4) | eItsMe ) << 16) | ((  (eItsMe << 4) | eItsMe ) << 8) | (eItsMe << 4) | eItsMe ;
            _states[5]  = ((  ((  (eItsMe << 4) | eItsMe  ) << 8) | (eItsMe << 4) | eItsMe ) << 16) | ((  (eItsMe << 4) | eItsMe ) << 8) | (eItsMe << 4) | eItsMe ;
            _states[6]  = ((  ((  (eError << 4) | eError  ) << 8) | (5      << 4) |      5 ) << 16) | ((  (5      << 4) |      5 ) << 8) | (eError << 4) | eError ;
            _states[7]  = ((  ((  (eError << 4) | eError  ) << 8) | (eError << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[8]  = ((  ((  (eError << 4) | eError  ) << 8) | (5      << 4) |      5 ) << 16) | ((  (5      << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[9]  = ((  ((  (eError << 4) | eError  ) << 8) | (eError << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[10] = ((  ((  (eError << 4) | eError  ) << 8) | (7      << 4) |      7 ) << 16) | ((  (7      << 4) |      7 ) << 8) | (eError << 4) | eError ;
            _states[11] = ((  ((  (eError << 4) | eError  ) << 8) | (eError << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[12] = ((  ((  (eError << 4) | eError  ) << 8) | (7      << 4) |      7 ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[13] = ((  ((  (eError << 4) | eError  ) << 8) | (eError << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[14] = ((  ((  (eError << 4) | eError  ) << 8) | (9      << 4) |      9 ) << 16) | ((  (9      << 4) |      9 ) << 8) | (eError << 4) | eError ;
            _states[15] = ((  ((  (eError << 4) | eError  ) << 8) | (eError << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[16] = ((  ((  (eError << 4) | eError  ) << 8) | (9      << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[17] = ((  ((  (eError << 4) | eError  ) << 8) | (eError << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[18] = ((  ((  (eError << 4) | eError  ) << 8) | (12     << 4) |     12 ) << 16) | ((  (12     << 4) |     12 ) << 8) | (eError << 4) | eError ;
            _states[19] = ((  ((  (eError << 4) | eError  ) << 8) | (eError << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[20] = ((  ((  (eError << 4) | eError  ) << 8) | (12     << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[21] = ((  ((  (eError << 4) | eError  ) << 8) | (eError << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[22] = ((  ((  (eError << 4) | eError  ) << 8) | (eError << 4) |     12 ) << 16) | ((  (12     << 4) |     12 ) << 8) | (eError << 4) | eError ;
            _states[23] = ((  ((  (eError << 4) | eError  ) << 8) | (eError << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;
            _states[24] = ((  ((  (eError << 4) | eError  ) << 8) | (eStart << 4) | eStart ) << 16) | ((  (eStart << 4) | eStart ) << 8) | (eError << 4) | eError ;
            _states[25] = ((  ((  (eError << 4) | eError  ) << 8) | (eError << 4) | eError ) << 16) | ((  (eError << 4) | eError ) << 8) | (eError << 4) | eError ;

            _charset  =  "UTF-8";
            _stFactor =  16;
        }

        public override bool isUCS2() => false;
    }
}
