using System;
using System.IO;

namespace ETS2.Unpacker
{
    class Program
    {
        private static String m_Title = "Euro Truck Simulator 2 SCS Unpacker";

        static void Main(String[] args)
        {
            Console.Title = m_Title;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(m_Title);
            Console.WriteLine("(c) 2022 Ekey (h4x0r) / v{0}\n", Utils.iGetApplicationVersion());
            Console.ResetColor();

            if (args.Length != 2)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    ETS2.Unpacker <m_File> <m_Directory>\n");
                Console.WriteLine("    m_File - Source of SCS archive file");
                Console.WriteLine("    m_Directory - Destination directory\n");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    ETS2.Unpacker E:\\Games\\ETS2\\base.scs D:\\Unpacked");
                Console.ResetColor();
                return;
            }

            String m_ScsFile = args[0];
            String m_Output = Utils.iCheckArgumentsPath(args[1]);

            if (!File.Exists(m_ScsFile))
            {
                Utils.iSetError("[ERROR]: Input SCS file -> " + m_ScsFile + " <- does not exist");
                return;
            }

            ScsUnpack.iDoIt(m_ScsFile, m_Output);
        }
    }
}
