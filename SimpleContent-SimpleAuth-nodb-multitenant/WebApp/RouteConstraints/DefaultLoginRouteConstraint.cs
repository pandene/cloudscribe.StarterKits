using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebApp.RouteConstraints
{
    public class DefaultLoginRouteConstraint :IRouteConstraint
    {
        private readonly string _controller;
        private readonly string _action;

        public DefaultLoginRouteConstraint(string controller, string action=null)
        {
            _controller = controller;
            _action = action;
        }

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            //return false;
            return !((_controller == null || String.Compare(values["controller"].ToString(), _controller, true) == 0)
                && (_action == null || String.Compare(values["action"].ToString(), _action, true) == 0));
            //return !( !(_controller ==null ||  String.Compare(values["controller"].ToString(), _controller, true) != 0)
            //    ||!(_action==null || String.Compare(values["action"].ToString(), _action, true) != 0));
        }
    }
}

