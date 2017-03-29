using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToolsLib.AdvanceStructure;

namespace WCSCompress.Core.CPCompressor.Helpers
{
    static class Helpers_Context
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetLastXContexBytes(SlidingWindow sw, byte[] context, int multiplier, int offset)
        {
            int tmp = (context.Length - 1) * multiplier + 1 + offset;
            int startIndex = sw.GetCurrWindowSize() - tmp;
            int contextLen = context.Length;
            for (int i = contextLen-1; i>=0 ; i--)
            //    for (int i = 0; i < contextLen; i++)
            {
                context[i] = sw.GetWindowByte(startIndex  );

                startIndex += multiplier;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MaxRealContextLenght(int contextLenght, int multiplier, int offset)
        {
            return (contextLenght - 1) * multiplier + 1 + offset;
        }

        public static int GetMaxPossibleContextSize(int historyContextSize, int contextLength, int multiplier, int offset)
        {
            int maxContext = contextLength;
            if (historyContextSize < Helpers_Context.MaxRealContextLenght(contextLength, multiplier, offset))
            {
                if (offset > historyContextSize) maxContext = 0;
                else
                    maxContext = (historyContextSize - offset) / multiplier;
            }

            return maxContext;
        }

        public static void GetLastXContexBytes2(SlidingWindow sw, byte[] context, int multiplier, int offset)
        {
            //int tmpOffset = multiplier - 1 - offset;
            //int len = sw.GetCurrWindowSize();
            int startIndex = sw.GetCurrWindowSize() - context.Length * multiplier + offset;
            int contextLen = context.Length;
            for (int i = 0; i < contextLen; i++)
            {
                context[i] = sw.GetWindowByte(startIndex);

                startIndex += multiplier;
            }
        }
    }
}
