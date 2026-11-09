using System.Web;
using System.Web.Mvc;

namespace _25DH110249_Trần_Anh_Tú
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
