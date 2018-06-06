using System;

namespace Command {
    class CmdException : Exception {
        public Code code;
        public string message;

        public CmdException ( Code code, string message ) {
            this.code = code;
            this.message = message;
        }

        public enum Code {
            success,    // 成功
            paramMiss,  // 缺少参数
            outofRange, // 超出范围
        }
    }
}
