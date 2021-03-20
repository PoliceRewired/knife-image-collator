using System;
namespace ImageCollatorLib.Helpers
{
    public class EnumHelper
    {
        public static E EnsureArgument<E>(string arg, string argName) where E : struct
        {
            E argument;
            var ok = Enum.TryParse(arg, out argument);
            if (ok)
            {
                return argument;
            }
            else
            {
                throw new ArgumentException(string.Format("Urecognised: {0}", arg), argName);
            }
        }
    }
}
