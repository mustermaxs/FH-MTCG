using System;
using System.Collections.Generic;
using System.Reflection;

namespace MTCG;
    public class EndpointBuilder
    {
        private HTTPMethod httpMethod;
        private string routePattern;
        private string routeTemplate;
        private Type controllerType;
        private MethodInfo controllerMethod;
        private Role accessLevel;
        private IUrlParams urlParams;

        public EndpointBuilder WithHttpMethod(HTTPMethod method)
        {
            httpMethod = method;
            return this;
        }

        public EndpointBuilder WithRoutePattern(string pattern)
        {
            routePattern = pattern;
            return this;
        }

        public EndpointBuilder WithRouteTemplate(string template)
        {
            routeTemplate = template;
            return this;
        }

        public EndpointBuilder WithControllerType(Type type)
        {
            if (!typeof(BaseController).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Invalid controller type passed.\nType passed: {type}");
            }
            controllerType = type;
            return this;
        }

        public EndpointBuilder WithControllerMethod(MethodInfo method)
        {
            controllerMethod = method;
            return this;
        }

        public EndpointBuilder WithAccessLevel(Role level)
        {
            accessLevel = level;
            return this;
        }

        public EndpointBuilder WithUrlParams(IUrlParams parameters)
        {
            urlParams = parameters;
            return this;
        }

        public Endpoint Build()
        {
            return new Endpoint(httpMethod, routePattern, routeTemplate, controllerType, controllerMethod, accessLevel)
            {
                UrlParams = urlParams
            };
        }
    }
