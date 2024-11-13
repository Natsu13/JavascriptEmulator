namespace JavascriptEmulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /*
                LOAD_GLOBAL Math    // Načte globální objekt Math
                LOAD_PROPERTY sqrt  // Načte metodu sqrt z objektu Math
                LOAD_CONSTANT 9     // Načte hodnotu 9
                CALL_FUNCTION 1     // Zavolá funkci sqrt s 1 argumentem
             */
            var emulator = new JavascriptEmulator();
            //emulator.Execute("var test = Math.floor(Date.now() / 1000);");
            emulator.Execute("var test = 5 * 20;");            
        }
    }
}
