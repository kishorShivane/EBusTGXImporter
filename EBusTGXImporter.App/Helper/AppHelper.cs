namespace EBusTGXImporter.Helpers
{
    public class AppHelper
    {
        public static bool IsXmlFile(string strToCheck)
        {
            bool result = false;

            if (strToCheck.ToUpper().Contains(".XML")) result = true;
            return result;
        }

        public static bool IsCsvFile(string strToCheck)
        {
            bool result = false;

            if (strToCheck.ToUpper().Contains(".CSV")) result = true;
            return result;
        }
    }
}
