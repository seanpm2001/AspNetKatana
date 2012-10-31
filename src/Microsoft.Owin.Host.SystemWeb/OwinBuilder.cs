﻿// <copyright file="OwinBuilder.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Owin;
using Owin.Builder;
using Owin.Loader;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal static class OwinBuilder
    {
        public static readonly Func<IDictionary<string, object>, Task> NotFound = env =>
        {
            env["owin.ResponseStatusCode"] = 404;
            return TaskHelpers.Completed();
        };

        public static Func<IDictionary<string, object>, Task> Build()
        {
            var configuration = ConfigurationManager.AppSettings["owin:Configuration"];
            var loader = new DefaultLoader();
            var startup = loader.Load(configuration ?? string.Empty);
            return Build(startup);
        }

        public static Func<IDictionary<string, object>, Task> Build(Action<IAppBuilder> startup)
        {
            if (startup == null)
            {
                return null;
            }

            var builder = new AppBuilder();
            builder.Properties["builder.DefaultApp"] = NotFound;
            builder.Properties["host.TraceOutput"] = TraceTextWriter.Instance;
            builder.Properties["host.AppName"] = HostingEnvironment.SiteName;
            builder.Properties["host.OnAppDisposing"] = new Action<Action>(callback => OwinApplication.ShutdownToken.Register(callback));

            var capabilities = new Dictionary<string, object>();
            builder.Properties[Constants.ServerCapabilitiesKey] = capabilities;

            capabilities[Constants.ServerNameKey] = Constants.ServerName;
            capabilities[Constants.ServerVersionKey] = Constants.ServerVersion;
            capabilities[Constants.SendFileVersionKey] = Constants.SendFileVersion;

            builder.UseFunc(next => env =>
            {
                env[Constants.ServerCapabilitiesKey] = capabilities;
                return next(env);
            });

            DetectWebSocketSupport(builder);
            startup(builder);
            return builder.Build<Func<IDictionary<string, object>, Task>>();
        }

        private static void DetectWebSocketSupport(IAppBuilder builder)
        {
#if NET45
            // There is no explicit API to detect server side websockets, just check for v4.5 / Win8.
            // Per request we can provide actual verification.
            if (HttpRuntime.IISVersion != null && HttpRuntime.IISVersion.Major >= 8)
            {
                var capabilities = builder.Properties.Get<IDictionary<string, object>>(Constants.ServerCapabilitiesKey);
                capabilities[Constants.WebSocketVersionKey] = Constants.WebSocketVersion;
            }
            else
#endif
            {
                // TODO: Trace
            }
        }
    }
}