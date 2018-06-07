﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AustinHarris.JsonRpc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Command.Parser {
    class JsonRpcParser : IParser {
        public object[] getArgs ( MethodInfo info, CmdParam param, CmdAttribute attr ) {
            var fparam = info.GetParameters ();
            object[] args = new object[fparam.Length];

            if (attr == null) {
                Debug.Fail ("Must apply a [MsgAttribute] to method");
                return args;
            }
            // 沒有参数
            if (fparam.Length == 0) {
                return args;
            }

            for (int i = 0; i<fparam.Length; i++) {
                var f = fparam[i];
                // 单个参数
                if (f.ParameterType == typeof (string))
                    args[i] = param[i];
                // 多个参数
                else if (f.ParameterType == typeof (List<string>))
                    args[i] = param[i, true];
                else
                    System.Console.WriteLine (string.Format ("Error parameter type : {0} {1}", f.Name, f.ParameterType));

                var mpa = f.GetCustomAttribute<CmdParamAttribute> ();
                // 设置了属性
                if (mpa != null) {
                    if (args[i] == null) {
                        if (mpa.require) { throw new ArgumentException ("Require parameter : {0}!".format (fparam[i].Name)); }// 需要的参数
                        else if (mpa.defaultValue != null) { args[i] = mpa.defaultValue; }// 默认值
                    }
                    else {
                        if (mpa.isswitch) { args[i] = "true"; } // 开关
                    }
                }
            }

            return args;
        }

        public CmdParam parseArgs ( string[] array ) {
            throw new NotImplementedException ();
        }

        public CmdParam parseArgs ( string str, int startIndex = 0 ) {
            JsonRequest req = null;
            try {
                req = JsonConvert.DeserializeObject<JsonRequest> (str);
                if (req == null) { return null; }
            }
            catch (Exception ex) {
                new JsonRpcException (-32700, "Parse error", ex);
            }
            CmdParam param = new CmdParam ();
            param.name = req.Method;
            param.data = req;

            if (req.Params is string)
                param[0] = req.Params as string;
            else {
                JObject obj = req.Params as JObject;

                int i = 0;
                var en = obj.GetEnumerator ();
                while (en.MoveNext ()) {
                    if (en.Current.Value.Type == JTokenType.String)
                        param[i] = en.Current.Value.ToString ();
                    else
                        foreach (var item in en.Current.Value) {
                            param[i] = item.ToString ();
                        }
                }
            }

            return param;
        }

        public string unParser ( string ret, int code = 0, object data = null ) {

            JsonRequest jsonRequest = data as JsonRequest;
            JsonResponse jsonResponse = new JsonResponse ();

            jsonResponse.Id = jsonRequest.Id;

            if (code != 0) {
                jsonResponse.Error = new JsonRpcException (code, ret, null);
            }
            else {
                jsonResponse.Result = ret;
            }

            if (jsonResponse.Result == null && jsonResponse.Error == null) {
                // Per json rpc 2.0 spec
                // result : This member is REQUIRED on success.
                // This member MUST NOT exist if there was an error invoking the method.    
                // Either the result member or error member MUST be included, but both members MUST NOT be included.
                jsonResponse.Result = new Newtonsoft.Json.Linq.JValue ((Object)null);
            }

            StringWriter sw = new StringWriter ();
            JsonTextWriter writer = new JsonTextWriter (sw);
            writer.WriteStartObject ();
            writer.WritePropertyName ("jsonrpc");
            writer.WriteValue ("2.0");

            if (jsonResponse.Error != null) {
                writer.WritePropertyName ("error");
                writer.WriteRawValue (JsonConvert.SerializeObject (jsonResponse.Error));
            }
            else {
                writer.WritePropertyName ("result");
                writer.WriteRawValue (JsonConvert.SerializeObject (jsonResponse.Result));
            }
            writer.WritePropertyName ("id");
            writer.WriteValue (jsonResponse.Id);
            writer.WriteEndObject ();
            return sw.ToString ();
        }
    }
}