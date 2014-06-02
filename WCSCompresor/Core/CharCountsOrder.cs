using System;
using System.Collections.Generic;
using System.Text;

namespace WCSCompress.Core
{
    class CharCountsOrder
    {
        public const int size_stack = 256;
        /// <summary>
        /// indexuje se bajtem, a vysledek je pozice kde se nachazi na zasobniku
        /// </summary>
        public byte [] Chars;  
        /// <summary>
        /// indexuje se poradim v yasobniku, 0 je nejvice cetny
        /// hodnota je bytektery je na te pozici
        /// </summary>
        public byte [] Stack;
        /// <summary>
        /// aktualni index cetnosti koresponduje s chars
        /// </summary>
        public int  [] Cetnost;

        public int Celk_Cetnost;

        public CharCountsOrder()
        {
            Chars = new byte[size_stack];
            Stack = new byte[size_stack];
            Cetnost = new int[size_stack];
            Init();
        }

        /// <summary>
        /// nastavi zasobnikove kodovani do pocatecniho stavu
        /// </summary>
        public void Init()
        {
            for (int i = 0; i < size_stack; i++)
            {
                Chars[i] = (byte)i;
                Stack[i] = (byte)i;
                Cetnost[i] = 0;
            }

            Celk_Cetnost = 0;
        }

        /// <summary>
        /// prida znak do zasobniku
        /// </summary>
        /// <param name="_char"></param>
        public void Add_Char(byte _char)
        {
            /// updatuje cetnost
            Cetnost[Chars[_char]]++;
            Celk_Cetnost++;
            
            byte i = Chars[_char];
            byte pom_stack = Stack[i];
            int pom_cetnost = Cetnost[i];

            while (i > 0 && pom_cetnost >= Cetnost[i-1])
            {
                Stack[i] = Stack[i - 1];
                Cetnost[i] = Cetnost[i - 1];

                Chars[Stack[i]]++;
                --i;
            }

            Stack[i] = pom_stack;
            Cetnost[i] = pom_cetnost;
            Chars[pom_stack] = i;
        }

        public byte Get_Pozice_In_Stack(byte _char) 
        {
            return Chars[_char];
        }

        public int Get_Cetnost_Char(byte _char)
        {
            return Cetnost[Chars[_char]];
        }

        public byte Get_Char_From_Poz_Stack(byte _poz)
        {
            return Stack[_poz];
        }
    }
}
