// <copyright file="MachineKeyDataProtecter.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

using System.Text;
using System.Web.Security;
using Microsoft.Owin.Security.DataProtection;

namespace Microsoft.Owin.Host.SystemWeb.DataProtection
{
    internal class MachineKeyDataProtecter : IDataProtecter
    {
        private readonly string[] _purposes;

        public MachineKeyDataProtecter(params string[] purposes)
        {
            _purposes = purposes;
        }

        public byte[] Protect(byte[] userData)
        {
#if NET40
            return Encoding.UTF8.GetBytes(MachineKey.Encode(userData, MachineKeyProtection.All));
#endif
#if NET45
            return MachineKey.Protect(userData, _purposes);
#endif
        }

        public byte[] Unprotect(byte[] protectedData)
        {
#if NET40
            return MachineKey.Decode(Encoding.UTF8.GetString(protectedData), MachineKeyProtection.All);
#endif
#if NET45
            return MachineKey.Unprotect(protectedData, _purposes);
#endif
        }
    }
}