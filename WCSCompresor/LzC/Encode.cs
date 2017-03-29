using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WCSCompress.LzC
{
    public class Encode
    {
        private Stream _output;

        public Encode(Stream output)
        {
            this._output = output;
        }

        public void EncodeMatch(int offset, int length, bool hasMark,int missOffset, int countMissChar, LzC_Core.MatchResult.MatchType type)
        {
            string kk;
            if (hasMark)
            {
                if (type == LzC_Core.MatchResult.MatchType.Pure) kk = $"[{offset.ToString()},{length.ToString()}]";
                else if (type == LzC_Core.MatchResult.MatchType.XSeqDifferent) kk = $"#{offset.ToString()},|{missOffset.ToString()},{countMissChar.ToString()}|,{length.ToString()}#";
                else if (type == LzC_Core.MatchResult.MatchType.LABXSeqRedundant) kk = $"@{offset},|{missOffset.ToString()},{countMissChar.ToString()}|,{length}@";
                else if (type == LzC_Core.MatchResult.MatchType.LABXSeqMissing) kk = $"%{offset},|{missOffset.ToString()},{countMissChar.ToString()}|,{length}%";
                else kk = $"#{offset},{length}#";
            }
            else kk = $"[{offset.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)},{length.ToString()}]";

            byte[] tmp = ASCIIEncoding.ASCII.GetBytes(kk);

            _output.Write(tmp, 0, tmp.Length);

            //_output.WriteByte((byte)'#');
        }

        public void EncodeRaw(byte data)
        {
            _output.WriteByte(data);
        }

        public void EncodeRaw(byte [] data, int offset, int length)
        {
            _output.Write(data,offset,length);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetEncodePosition_Size(int position)
        {
            int pos = position;

            if (pos < 257) return 1;
            if ((1 << 15) + 1 > pos) return 2;
            else return 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetEncodeLenght_Size(int lenght)
        {
            if (lenght < 20) return 1;
            else return 2;
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short GetEncodeSize_PureMatch(int position, int lenght, int bufferLen)
        {
            int posSize = GetEncodePosition_Size(bufferLen - (position + lenght));
            int lengthSize = GetEncodeLenght_Size(lenght);
            return (short)(posSize + lengthSize);
        }

        
        /// <summary>
        /// koduje ze pozice a delka standartne, a offset s poctem po sobe jdoucich chybnych do jednoho byte, + znaky chybne
        /// </summary>
        /// <param name="position"></param>
        /// <param name="lenght"></param>
        /// <param name="offsetMissMatch"></param>
        /// <param name="countMissMatch"></param>
        /// <param name="bufferLen"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short GetEncodeSize_XTolerance_Match(int position, int lenght, int offsetMissMatch, int countMissMatch, int bufferLen)
        {
            int posSize = GetEncodePosition_Size(bufferLen - (position + lenght));
            int lengthSize = GetEncodeLenght_Size(lenght);

            int offsetAndCountMissMatch = 1;
            return (short)(posSize + lengthSize + offsetAndCountMissMatch + countMissMatch);
        }

        /// <summary>
        /// koduje ze pozice a delka standartne, a offset s poctem po sobe jdoucich chybnych do jednoho byte, + znaky chybne
        /// </summary>
        /// <param name="position"></param>
        /// <param name="lenght"></param>
        /// <param name="offsetMissMatch"></param>
        /// <param name="countMissMatch"></param>
        /// <param name="bufferLen"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short GetEncodeSize_lbXRedundant_Match(int position, int lenght, int offsetMissMatch, int countMissMatch, int bufferLen)
        {
            int posSize = GetEncodePosition_Size(bufferLen - (position + lenght-countMissMatch ));
            int lengthSize = GetEncodeLenght_Size(lenght);

            int offsetAndCountMissMatch = 1;
            return (short)(posSize + lengthSize + offsetAndCountMissMatch + countMissMatch);
        }

        /// <summary>
        /// koduje ze pozice a delka standartne, a offset s poctem po sobe jdoucich chybnych do jednoho byte, + znaky chybne
        /// </summary>
        /// <param name="position"></param>
        /// <param name="lenght"></param>
        /// <param name="offsetMissMatch"></param>
        /// <param name="countMissMatch"></param>
        /// <param name="bufferLen"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short GetEncodeSize_lbXMissing_Match(int position, int lenght, int offsetMissMatch, int countMissMatch, int bufferLen)
        {
            int posSize = GetEncodePosition_Size(bufferLen - (position + lenght+countMissMatch ));
            int lengthSize = GetEncodeLenght_Size(lenght);

            int offsetAndCountMissMatch = 2;
            return (short)(posSize + lengthSize + offsetAndCountMissMatch );
        }


        



    }
}
