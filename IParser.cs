﻿using System;
using System.Reflection;

namespace Command {
    public interface IParser {
        /// <summary>
        /// 分配参数到函数
        /// </summary>
        object[] getArgs ( MethodInfo info, CmdParam param, CmdAttribute attr );

        /// <summary>
        /// 解析参数
        /// </summary>
        CmdParam parseArgs ( string[] array );

        /// <summary>
        /// 预处理参数参数
        /// </summary>
        string[] preParseArgs ( string str, int startIndex = 0 );
    }
}
