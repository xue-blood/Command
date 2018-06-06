using System;
using System.Collections.Generic;
using System.Reflection;

namespace Command {
    public static class CmdHandleExtension {
        public static object Run<T> ( this MethodInfo info, string[] args, string plugin = "Console" ) {
            CmdHandle handle = new CmdHandle ();
            handle.Init (plugin);
            handle.Read (args);
            return handle.Run<T> (info);
        }

        public static object Run<T> ( this MethodInfo info, string arg, string plugin = "Console", int startIndex = 0 ) {
            CmdHandle handle = new CmdHandle ();
            handle.Init (plugin);
            handle.Read (arg, startIndex);
            return handle.Run<T> (info);
        }
    }
}
